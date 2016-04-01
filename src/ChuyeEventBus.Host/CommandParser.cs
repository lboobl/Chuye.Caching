using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;

namespace ChuyeEventBus.Host {

    public class CommandParser {
        public CommandParserSetting Setting { get; set; }

        public CommandParser() {
            Setting = new CommandParserSetting(StartChar.Any, SplitChar.Any);
        }

        public CommandParser(CommandParserSetting setting) {
            Setting = setting;
        }

        public IEnumerable<KeyValuePair<String, String>> Parse(String[] args) {
            foreach (var arg in args) {
                if (!String.IsNullOrWhiteSpace(arg) && arg.Length > 1 && Setting.ValidStart(arg[0])) {
                    Int32 position = 1;
                    while (position < arg.Length && !Setting.ValidSplit(arg[position])) {
                        position++;
                    }
                    if (position > 1) {
                        String key = arg.Substring(1, position - 1);
                        String value = null;
                        if (position < arg.Length - 1) {
                            value = arg.Substring(position + 1, arg.Length - 1 - position);
                        }
                        yield return new KeyValuePair<String, String>(key, value);
                    }
                }
            }
        }

        public Dictionary<String, String> ParseAsDict(String[] args) {
            return Parse(args).ToDictionary(p => p.Key, p => p.Value);
        }

        public NameValueCollection ParseAsForm(String[] args) {
            NameValueCollection form = new NameValueCollection();
            foreach (var item in Parse(args)) {
                form.Set(item.Key, item.Value);
            }
            return form;
        }

        public String Combin(IEnumerable<KeyValuePair<String, String>> pairs) {
            if (pairs.Any()) {
                StringBuilder builder = new StringBuilder();
                Char start = Setting.GetStartChar();
                Char split = Setting.GetSplitChar();
                foreach (var pair in pairs) {
                    builder.Append(start);
                    Append(builder, pair.Key);
                    builder.Append(split);
                    Append(builder, pair.Value);
                    builder.Append(' ');
                }
                builder.Remove(builder.Length - 1, 1);
                return builder.ToString();
            }
            return String.Empty;
        }

        private static void Append(StringBuilder builder, String value) {
            Boolean needQuote = value.Contains(' ');
            if (needQuote) {
                builder.Append('"');
            }
            builder.Append(value);
            if (needQuote) {
                builder.Append('"');
            }
        }

        public String Combin(Object input) {
            var pairs = TypeDescriptor.GetProperties(input).OfType<PropertyDescriptor>()
                .Select(p => new KeyValuePair<String, String>(p.Name, p.GetValue(input).ToString()));
            return Combin(pairs);
        }
    }



    [Flags]
    public enum StartChar {
        Slash = 0x1,
        Dash = 0x2,
        Any = Slash | Dash,
    }

    [Flags]
    public enum SplitChar {
        Equals = 0x1,
        Colon = 0x2,
        Any = Equals | Colon
    }

    public class CommandParserSetting {
        public StartChar Start { get; set; }
        public SplitChar Split { get; set; }

        public CommandParserSetting() {
            Start = StartChar.Any;
            Split = SplitChar.Any;
        }

        public CommandParserSetting(StartChar start, SplitChar split) {
            Start = start;
            Split = split;
        }

        public Char GetStartChar() {
            switch (Start) {
                case StartChar.Slash:
                case StartChar.Any:
                    return '/';
                case StartChar.Dash:
                    return '-';
            }
            throw new Exception("Undefined StartChar: " + Start);
        }

        public Char GetSplitChar() {
            switch (Split) {
                case SplitChar.Colon:
                case SplitChar.Any:
                    return ':';
                case SplitChar.Equals:
                    return '=';
            }
            throw new Exception("Undefined SplitChar: " + Split);
        }

        public Boolean ValidStart(Char ch) {
            switch (Start) {
                case StartChar.Slash:
                    return ch == '/';
                case StartChar.Dash:
                    return ch == '-';
                case StartChar.Any:
                    return ch == '/' || ch == '-';
            }
            return false;
        }

        public Boolean ValidSplit(Char ch) {
            switch (Split) {
                case SplitChar.Equals:
                    return ch == '=';
                case SplitChar.Colon:
                    return ch == ':';
                case SplitChar.Any:
                    return ch == '=' || ch == ':';
            }
            return false;
        }
    }



    public class CommandLineSpliter {
        [SecurityCritical]
        public IEnumerable<string> Parse(string commandLine) {
            return SplitArgs(commandLine);
        }

        public class State {
            private readonly string _commandLine;
            private readonly StringBuilder _stringBuilder;
            private readonly List<string> _arguments;
            private int _index;

            public State(string commandLine) {
                _commandLine = commandLine;
                _stringBuilder = new StringBuilder();
                _arguments = new List<string>();
            }

            public StringBuilder StringBuilder { get { return _stringBuilder; } }
            public bool EOF { get { return _index >= _commandLine.Length; } }
            public char Current { get { return _commandLine[_index]; } }
            public IEnumerable<string> Arguments { get { return _arguments; } }

            public void AddArgument() {
                _arguments.Add(StringBuilder.ToString());
                StringBuilder.Clear();
            }

            public void AppendCurrent() {
                StringBuilder.Append(Current);
            }

            public void Append(char ch) {
                StringBuilder.Append(ch);
            }

            public void MoveNext() {
                if (!EOF)
                    _index++;
            }
        }

        /// <summary>
        /// Implement the same logic as found at
        /// http://msdn.microsoft.com/en-us/library/17w5ykft.aspx
        /// The 3 special characters are quote, backslash and whitespaces, in order 
        /// of priority.
        /// The semantics of a quote is: whatever the state of the lexer, copy
        /// all characters verbatim until the next quote or EOF.
        /// The semantics of backslash is: If the next character is a backslash or a quote,
        /// copy the next character. Otherwise, copy the backslash and the next character.
        /// The semantics of whitespace is: end the current argument and move on to the next one.
        /// </summary>
        private IEnumerable<string> SplitArgs(string commandLine) {
            var state = new State(commandLine);
            while (!state.EOF) {
                switch (state.Current) {
                    case '"':
                        ProcessQuote(state);
                        break;

                    case '\\':
                        ProcessBackslash(state);
                        break;

                    case ' ':
                    case '\t':
                        if (state.StringBuilder.Length > 0)
                            state.AddArgument();
                        state.MoveNext();
                        break;

                    default:
                        state.AppendCurrent();
                        state.MoveNext();
                        break;
                }
            }
            if (state.StringBuilder.Length > 0)
                state.AddArgument();
            return state.Arguments;
        }

        private void ProcessQuote(State state) {
            state.MoveNext();
            while (!state.EOF) {
                if (state.Current == '"') {
                    state.MoveNext();
                    break;
                }
                state.AppendCurrent();
                state.MoveNext();
            }

            state.AddArgument();
        }

        private void ProcessBackslash(State state) {
            state.MoveNext();
            if (state.EOF) {
                state.Append('\\');
                return;
            }

            if (state.Current == '"') {
                state.Append('"');
                state.MoveNext();
            }
            else {
                state.Append('\\');
                state.AppendCurrent();
                state.MoveNext();
            }
        }
    }
}
