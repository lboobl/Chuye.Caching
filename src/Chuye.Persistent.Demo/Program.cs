using Chuye.Persistent.Mongo;
using Chuye.Persistent.NH;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace Chuye.Persistent.Demo {
    class Program {
        static void Main(string[] args) {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            HasOne(); return;
            PrepareData();
            BasicCrud();
        }

        private static void HasOne() {
            var context = new PubsContext();
            context.Begin();

            var jobRepo = new NHibernateRepository<Job>(context);
            var employeeRepo = new NHibernateRepository<Employee>(context);

            var job = jobRepo.Retrive(44);
            //var employee = employeeRepo.All.Where(e => e.Job == job).ToList();
            //Console.WriteLine(employee.Count);

            var employee = employeeRepo.All.Where(e => e.Job.Id == job.Id).ToList();
            Console.WriteLine(employee.Count);
            
        }

        private static MongoRepository<Employee> MongoRepository() {
            var conStr = ConfigurationManager.ConnectionStrings["PubsMongo"].ConnectionString;
            var context = new MongoRepositoryContext(conStr);
            var repository = new MongoRepository<Employee>(context);
            return repository;
        }


        private static void PrepareData() {
            var conStr = ConfigurationManager.ConnectionStrings["PubsMongo"].ConnectionString;
            var context = new MongoRepositoryContext(conStr);
            var repository = new MongoRepository<Employee>(context);

            var docs = context.Database.GetCollection<Employee>();
            Console.WriteLine("Remove all employee");
            docs.RemoveAll();
            Console.WriteLine();

            var names = "Charles、Mark、Bill、Vincent、William、Joseph、James、Henry、Gary、Martin"
               .Split('、', ' ');
            for (int i = 0; i < names.Length; i++) {
                var entry = new Employee {
                    Name = names[i],
                    Address = Guid.NewGuid().ToString().Substring(0, 8),
                    Birth = DateTime.UtcNow,
                    Job = new Job {
                        Title = Guid.NewGuid().ToString().Substring(0, 8),
                        Salary = Math.Abs(Guid.NewGuid().GetHashCode() % 8000)
                    }
                };
                repository.Create(entry);
            }

            Console.WriteLine("Query all employee");
            foreach (var entry in repository.All.Where(r => r.Job.Salary > 3000)) {
                Console.WriteLine("{0,-10} {1}", entry.Name, entry.Job.Salary);
            }
            Console.WriteLine();
        }

        private static void BasicCrud() {
            var conStr = ConfigurationManager.ConnectionStrings["PubsMongo"].ConnectionString;
            var context = new MongoRepositoryContext(conStr);
            var repository = new MongoRepository<Employee>(context);

            Console.WriteLine("Remove all employee");
            foreach (var entry in repository.All) {
                repository.Delete(entry);
            }
            Console.WriteLine();

            Console.WriteLine("Create employee");
            var Aimee = new Employee {
                Name = "Aimee",
                Address = "Los Angeles",
                Birth = DateTime.Now,
                Job = new Job {
                    Title = "C#",
                    Salary = 4
                }
            };
            repository.Save(Aimee);

            repository.Retrive(Aimee.Id);

            var Becky = new Employee {
                Name = "Becky",
                Address = "Bejing",
                Birth = DateTime.Now,
                Job = new Job {
                    Title = "Java",
                    Salary = 5
                }
            };
            repository.Create(Becky);
            var Carmen = new Employee {
                Name = "Carmen",
                Address = "Salt Lake City",
                Birth = DateTime.Now,
                Job = new Job {
                    Title = "Javascript",
                    Salary = 3
                }
            };
            repository.Create(Carmen);
            Console.WriteLine();

            Console.WriteLine("Update employee");
            Carmen = repository.Retrive(Carmen.Id);
            Carmen.Job.Title = "Java";
            Carmen.Job.Salary = 5;
            repository.Update(Carmen);
            Console.WriteLine();

            Console.WriteLine("Employee live in USA");
            foreach (var entry in repository.Retrive("Address", new[] { "Los Angeles", "Salt Lake City" })) {
                Console.WriteLine("{0,-10} {1} {2}",
                   entry.Name, entry.Job.Salary, entry.Address);
            }
            Console.WriteLine();

            Console.WriteLine("Employee live in specfied names");
            var names = repository.All.Select(r => r.Name).Take(5).ToArray();
            foreach (var entry in repository.Retrive(r => r.Name, names)) {
                Console.WriteLine("{0,-10} {1} {2}",
                   entry.Name, entry.Job.Salary, entry.Address);
            }
            Console.WriteLine();

            Console.WriteLine("Delete employee");
            repository.Delete(Carmen);
            Console.WriteLine("Employee left {0}", repository.All.Count());
        }
    }
}
