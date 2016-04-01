using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching.Tests {
    [Serializable]
    public class Person {
        public int Id { get; set; }
        public String Name { get; set; }
        public Address Address { get; set; }
    }

    [Serializable]
    public class Address {
        public String Line1 { get; set; }
        public String Line2 { get; set; }
    }
}
