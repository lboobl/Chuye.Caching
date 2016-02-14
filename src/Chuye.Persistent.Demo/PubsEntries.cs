using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chuye.Persistent;
using MongoDB.Bson;

namespace Chuye.Persistent.Demo {
    [PetaPoco.TableName("job")]
    public class Job : IAggregate
    {
        public virtual Int32                      Id                        { get; set; }  //pk, identity, int not null
        public virtual String                     Title                     { get; set; }  //varchar(50) not null
        public virtual Decimal                    Salary                    { get; set; }  //decimal(12,2) not null
    }

    [PetaPoco.TableName("employee")]
    public class Employee : IAggregate
    {
        public virtual Int32                      Id                        { get; set; }  //pk, identity, int not null
        public virtual String                     Name                      { get; set; }  //varchar(10) not null
        public virtual DateTime                   Birth                     { get; set; }  //datetime not null
        public virtual String                     Address                   { get; set; }  //varchar(255)
        [PetaPoco.Ignore]
        public virtual Job                        Job                       { get; set; }  //int not null
    }

    public class Department : IAggregate<Guid>
    {
        public virtual Guid                       Id                        { get; set; }  //pk, identity, int not null
        public virtual String                     Name                      { get; set; }  //varchar(50) not null
    }

    public class Shipper : IAggregate<ObjectId>
    {
        public virtual ObjectId                   Id                        { get; set; }  //pk, identity, int not null
        public virtual String                     CompanyName               { get; set; }  //varchar(50) not null
        public virtual String                     Phone                     { get; set; }  //varchar(50) not null
    }
}
