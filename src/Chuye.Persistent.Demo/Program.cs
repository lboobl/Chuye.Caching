using Chuye.Persistent.Mongo;
using Chuye.Persistent.NH;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace Chuye.Persistent.Demo {
    class Program {
        static void Main(string[] args) {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            //MongoBasicCrud();
            NHibernateBasicCrud();
        }

        private static void MongoBasicCrud() {
            var conStr = ConfigurationManager.ConnectionStrings["PubsMongo"].ConnectionString;
            var context = new MongoRepositoryContext(conStr);
            var employeeRepo = new MongoRepository<Employee>(context);

            Console.WriteLine("Remove all employee");
            context.Database.GetCollection<Employee>().RemoveAll();
            Console.WriteLine();

            var jobTitles = new[] { "Java", "C", "C++", "Objective-C", "C#", "JavaScript", "PHP", "Python" };
            var employeeNames = new[] { "Charles", "Mark", "Bill", "Vincent", "William", "Joseph", "James", "Henry", "Gary", "Martin" };

            for (int i = 0; i < employeeNames.Length; i++) {
                var entry = new Employee {
                    Name = employeeNames[i],
                    Address = Guid.NewGuid().ToString().Substring(0, 8),
                    Birth = DateTime.UtcNow,
                    Job = new Job {
                        Title = jobTitles[Math.Abs(Guid.NewGuid().GetHashCode() % jobTitles.Length)],
                        Salary = Math.Abs(Guid.NewGuid().GetHashCode() % 8000)
                    }
                };
                employeeRepo.Create(entry);
            }

            Console.WriteLine("Query all employee");
            foreach (var entry in employeeRepo.All.Where(r => r.Job.Salary > 3000)) {
                Console.WriteLine("{0,-10} {1}", entry.Name, entry.Job.Salary);
            }
            Console.WriteLine();
        }

        private static void NHibernateBasicCrud() {
            var context = new PubsContext();
            var jobRepo = new NHibernateRepository<Job>(context);
            var jobTitles = new[] { "Java", "C", "C++", "Objective-C", "C#", "JavaScript", "PHP", "Python" };

            context.Begin();
            Console.WriteLine("Remove all jobs");
            context.EnsureSession().CreateSQLQuery("delete from Employee").ExecuteUpdate();

            for (int i = 0; i < jobTitles.Length; i++) {
                var job = new Job {
                    Title = jobTitles[i],
                    Salary = Math.Abs(Guid.NewGuid().GetHashCode() % 8000 + 8000),
                };
                jobRepo.Create(job);
            }

            var jobs = jobRepo.All.ToList();
            var employeeRepo = new NHibernateRepository<Employee>(context);
            Console.WriteLine("Remove all employee");
            context.EnsureSession().CreateSQLQuery("delete from Employee").ExecuteUpdate();
            Console.WriteLine();

            var names = "Charles、Mark、Bill、Vincent、William、Joseph、James、Henry、Gary、Martin"
                .Split('、', ' ');
            for (int i = 0; i < names.Length; i++) {
                var entry = new Employee {
                    Name = names[i],
                    Address = Guid.NewGuid().ToString().Substring(0, 8),
                    Birth = DateTime.UtcNow,
                    Job = jobs[Math.Abs(Guid.NewGuid().GetHashCode() % jobs.Count)],
                };
                employeeRepo.Create(entry);
            }

            Console.WriteLine("Query all employee");
            foreach (var entry in employeeRepo.All.Where(r => r.Job.Salary > 3000)) {
                Console.WriteLine("{0,-10} {1}", entry.Name, entry.Job.Salary);
            }
            Console.WriteLine();

            context.Commit();
            context.Dispose();
        }
    }
}
