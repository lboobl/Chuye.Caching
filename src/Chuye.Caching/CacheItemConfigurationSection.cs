using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Chuye.Caching {
    public class CacheItemSharedSection : ConfigurationSection {
        private const String pattern = "pattern";
        private const String maxExpiration = "maxExpiration";
        private const String @readonly = "readonly";
        private const String leaveDashForEmtpyRegion = "leaveDashForEmtpyRegion";

        [ConfigurationProperty(pattern, IsRequired = true)]
        public String Pattern {
            get { return (String)this[pattern]; }
            set { this[pattern] = value; }
        }

        [ConfigurationProperty(maxExpiration)]
        public Double MaxExpiration {
            get { return (Double)this[maxExpiration]; }
            set { this[maxExpiration] = value; }
        }

        [ConfigurationProperty(@readonly)]
        public Boolean Readonly {
            get { return (Boolean)this[@readonly]; }
            set { this[@readonly] = value; }
        }

        [ConfigurationProperty(leaveDashForEmtpyRegion)]
        public Boolean LeaveDashForEmtpyRegion {
            get { return (Boolean)this[leaveDashForEmtpyRegion]; }
            set { this[leaveDashForEmtpyRegion] = value; }
        }
    }

    public class CacheItemConfigurationSection : CacheItemSharedSection {
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