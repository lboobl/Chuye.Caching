using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Chuye.Caching {
    public class CacheItemSharedSection : ConfigurationSection {
        private const String pattern = "pattern";
        private const String maxExpirationHour = "maxExpirationHour";
        private const String @readonly = "readonly";
        private const string formatNullRegion = "formatNullRegion";

        [ConfigurationProperty(pattern, IsRequired = true)]
        public String Pattern {
            get { return (String)this[pattern]; }
            set { this[pattern] = value; }
        }

        [ConfigurationProperty(maxExpirationHour)]
        public Double MaxExpirationHour {
            get { return (Double)this[maxExpirationHour]; }
            set { this[maxExpirationHour] = value; }
        }

        [ConfigurationProperty(@readonly)]
        public Boolean Readonly {
            get { return (Boolean)this[@readonly]; }
            set { this[@readonly] = value; }
        }

        [ConfigurationProperty(formatNullRegion)]
        public Boolean FormatNullRegion {
            get { return (Boolean)this[formatNullRegion]; }
            set { this[formatNullRegion] = value; }
        }
    }

    public class CacheConfigurationSection : CacheItemSharedSection {
        private const String details = "details";

        [ConfigurationCollection(typeof(CacheItemDetailElement), AddItemName = "add")]
        [ConfigurationProperty(details)]
        public CacheItemElementCollection Details {
            get { return (CacheItemElementCollection)this[details]; }
            set { this[details] = value; }
        }
    }

    public class CacheItemDetailElement : CacheItemSharedSection {
        private const String region = "region";
        private const String provider = "provider";

        [ConfigurationProperty(region)]
        public String Region {
            get { return (String)this[region]; }
            set { this[region] = value; }
        }

        [ConfigurationProperty(provider, IsRequired = true)]
        public String Provider {
            get { return (String)this[provider]; }
            set { this[provider] = value; }
        }
    }

    public class CacheItemElementCollection : ConfigurationElementCollection {
        protected override ConfigurationElement CreateNewElement() {
            return new CacheItemDetailElement();
        }

        protected override object GetElementKey(ConfigurationElement element) {
            var detail = (CacheItemDetailElement)element;
            return String.Format("{0}, {1}", detail.Provider, detail.Region);
        }

        public void Add(CacheItemDetailElement element) {
            base.BaseAdd(element);
        }

        internal CacheItemDetailElement Get(String provider) {
            return base.BaseGet(provider) as CacheItemDetailElement;
        }
    }
}