using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using SysCfg = System.Configuration.Configuration;
using System.Web;
using System.Web.Configuration;

namespace Chuye.Caching {
    public class ConfigurationResolver {
        private SysCfg configuration;
        private String exeConfigFilename = null;

        public SysCfg Configuration {
            get {
                if (HttpContext.Current != null) {
                    configuration = WebConfigurationManager.OpenWebConfiguration("~");
                }
                else { 
                    //configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    if (String.IsNullOrEmpty(exeConfigFilename)) {
                        configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    }
                    else {
                        ExeConfigurationFileMap file = new ExeConfigurationFileMap();
                        file.ExeConfigFilename = exeConfigFilename;
                        configuration = ConfigurationManager.OpenMappedExeConfiguration(file, ConfigurationUserLevel.None);
                    }
                }
                return configuration;
            }
        }

        public String ExeConfigFilename {
            get { return exeConfigFilename; }
            set {
                if (!String.IsNullOrWhiteSpace(value) && !value.Equals(exeConfigFilename, StringComparison.OrdinalIgnoreCase)) {
                    exeConfigFilename = value;
                    Refresh();
                }
            }
        }

        public T Read<T>() where T : ConfigurationSection {
            String sectionName = typeof(T).Name;
            return Read<T>(null, sectionName);
        }

        public T Read<T>(String sectionName) where T : ConfigurationSection {
            return Read<T>(null, sectionName);
        }

        public T Read<T>(String groupName, String sectionName) where T : ConfigurationSection {
            ConfigurationSection section = null;
            if (String.IsNullOrWhiteSpace(groupName)) {
                section = Configuration.Sections.Get(sectionName);
            }
            else {
                ConfigurationSectionGroup group = Configuration.SectionGroups.Get(groupName);
                if (group != null) {
                    section = group.Sections.Get(sectionName);
                }
            }
            // null is not T
            if (section != null && !(section is T)) {
                throw new InvalidCastException("Section read failed");
            }
            return (T)section;
        }

        public IEnumerable<T> ReadGroup<T>(String groupName) where T : ConfigurationSection {
            ConfigurationSectionGroup group = Configuration.SectionGroups.Get(groupName);
            if (group != null) {
                return group.Sections.OfType<T>();
            }
            return Enumerable.Empty<T>();
        }

        public void Refresh() {
            configuration = null;
        }

        public void Save<T>(T section) where T : ConfigurationSection {
            String sectionName = typeof(T).Name;
            Save<T>(section, sectionName, null);
        }

        public void Save<T>(T section, String sectionName) where T : ConfigurationSection {
            Save<T>(section, sectionName, null);
        }

        public void Save<T>(T section, String sectionName, String groupName) where T : ConfigurationSection {
            if (section == null) {
                throw new ArgumentNullException("section");
            }

            ConfigurationSectionCollection sections;
            if (String.IsNullOrWhiteSpace(groupName)) {
                sections = Configuration.Sections;
            }
            else {
                ConfigurationSectionGroup group = Configuration.SectionGroups.Get(groupName);
                if (group == null) {
                    Configuration.SectionGroups.Add(groupName, group = new ConfigurationSectionGroup());
                }
                sections = group.Sections;
            }

            T exist = (T)sections.Get(sectionName);
            if (exist == null) {
                sections.Add(sectionName, section);
            }
            else {
                if (exist != section) {
                    sections.Remove(sectionName);
                    sections.Add(sectionName, section);
                }
            }
            Configuration.Save(ConfigurationSaveMode.Minimal);
        }
    }
}