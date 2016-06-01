using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Chuye.Caching {
    public class CacheItemConfigurationSection : ConfigurationSection {
        private const String pattern = "pattern";
        private const String details = "details";
        private const String maxExpiration = "maxExpiration";
        private const String leaveDashForEmtpyRegion = "leaveDashForEmtpyRegion";

        [ConfigurationProperty(pattern, IsRequired = true)]
        public String Pattern {
            get { return (String)this[pattern]; }
            set { this[pattern] = value; }
        }

        [ConfigurationProperty(leaveDashForEmtpyRegion)]
        public Boolean LeaveDashForEmtpyRegion {
            get { return (Boolean)this[leaveDashForEmtpyRegion]; }
            set { this[leaveDashForEmtpyRegion] = value; }
        }

        [ConfigurationProperty(maxExpiration)]
        public Double MaxExpiration {
            get { return (Double)this[maxExpiration]; }
            set { this[maxExpiration] = value; }
        }

        [ConfigurationCollection(typeof(CacheItemDetailElement), AddItemName = "Add")]
        [ConfigurationProperty(details)]
        public CacheItemElementCollection Details {
            get { return (CacheItemElementCollection)this[details]; }
            set { this[details] = value; }
        }
    }

    public class CacheItemDetailElement : ConfigurationElement {
        private const String provider = "provider";
        private const String pattern = "pattern";
        private const String maxExpiration = "maxExpiration";
        private const String leaveDashForEmtpyRegion = "leaveDashForEmtpyRegion";

        [ConfigurationProperty(pattern, IsRequired = true)]
        public String Pattern {
            get { return (String)this[pattern]; }
            set { this[pattern] = value; }
        }

        [ConfigurationProperty(provider, IsRequired = true)]
        public String Provider {
            get { return (String)this[provider]; }
            set { this[provider] = value; }
        }

        [ConfigurationProperty(maxExpiration)]
        public Double MaxExpiration {
            get { return (Double)this[maxExpiration]; }
            set { this[maxExpiration] = value; }
        }

        [ConfigurationProperty(leaveDashForEmtpyRegion)]
        public Boolean LeaveDashForEmtpyRegion {
            get { return (Boolean)this[leaveDashForEmtpyRegion]; }
            set { this[leaveDashForEmtpyRegion] = value; }
        }
    }

    public class CacheItemElementCollection : ConfigurationElementCollection {
        protected override ConfigurationElement CreateNewElement() {
            return new CacheItemDetailElement();
        }

        protected override object GetElementKey(ConfigurationElement element) {
            return ((CacheItemDetailElement)element).Provider;
        }

        public void Add(CacheItemDetailElement element) {
            base.BaseAdd(element);
        }

        public CacheItemDetailElement Get(String provider) {
            return base.BaseGet(provider) as CacheItemDetailElement;
        }
    }
}