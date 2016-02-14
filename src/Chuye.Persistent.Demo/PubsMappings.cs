using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent.Demo {
    public class JobMap : ClassMap<Job> {
        public JobMap() {
            Id(x => x.Id)/*.GeneratedBy.Assigned()*/;
            Map(x => x.Title).Not.Nullable().Length(255);
            Map(x => x.Salary).Not.Nullable();
        }
    }

    public class EmployeeMap : ClassMap<Employee> {
        public EmployeeMap() {
            Id(x => x.Id)/*.GeneratedBy.Assigned()*/;
            Map(x => x.Name).Not.Nullable().Length(255);
            Map(x => x.Birth).Not.Nullable();
            Map(x => x.Address).Nullable();
            References(x => x.Job).Column("JobId").NotFound.Ignore();
            //HasOne(x => x.Job).ForeignKey("JobId");
        }
    }

    public class DepartmentMap : ClassMap<Department> {
        public DepartmentMap() {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.Name);
        }
    }
}

