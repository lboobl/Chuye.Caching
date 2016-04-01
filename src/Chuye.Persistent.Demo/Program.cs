using Chuye.Persistent.Mongo;
using Chuye.Persistent.NH;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Chuye.Persistent.Demo {
    class Program {
        static void Main(string[] args) {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            NHibernateBasicCrud();
            NHibernateGuidAggregateRoot();
            //NHibernate_Dupliate_entity_need_evict_and_Statistics();
            //NHibernate_Dupliate_entity_need_evict_before_update();

            MongoBasicCrud();
            MongoAggregateRoot();
        }

        private static void NHibernateBasicCrud() {
            var context = new PubsContext();
            context.Begin();

            //Delete from job
            Console.WriteLine("Remove all jobs");
            context.EnsureSession().CreateSQLQuery("delete from job").ExecuteUpdate();

            //Fill jobs
            var jobTitles = new[] { "Java", "C", "C++", "Objective-C", "C#", "JavaScript", "PHP", "Python" };
            var jobRepo = new NHibernateRepository<Job>(context);
            for (int i = 0; i < jobTitles.Length; i++) {
                var job = new Job {
                    Title = jobTitles[i],
                    Salary = Math.Abs(Guid.NewGuid().GetHashCode() % 8000 + 8000),
                };
                jobRepo.Create(job);
            }

            //delete from employee
            Console.WriteLine("Remove all employee");
            context.EnsureSession().CreateSQLQuery("delete from Employee").ExecuteUpdate();

            //query all jobs
            Console.WriteLine("Query part jobs");
            var jobs = jobRepo.Fetch(q => q.ToList());

            //query part jobs
            var halfJobTitles = jobTitles.Take(jobTitles.Length / 2).ToArray();
            var halfJobs = jobRepo.Retrive(j => j.Title, halfJobTitles);

            //Fill employee
            var employeeNames = "Charles、Mark、Bill、Vincent、William、Joseph、James、Henry、Gary、Martin"
             .Split('、', ' ');
            var employeeRepo = new NHibernateRepository<Employee>(context);
            for (int i = 0; i < employeeNames.Length; i++) {
                var entry = new Employee {
                    Name = employeeNames[i],
                    Address = Guid.NewGuid().ToString().Substring(0, 8),
                    Birth = DateTime.UtcNow,
                    Job = jobs[Math.Abs(Guid.NewGuid().GetHashCode() % jobs.Count)],
                };
                employeeRepo.Create(entry);
            }

            //query employee
            Console.WriteLine("Query employee where salary > 3000");
            var list = employeeRepo.Fetch(q => q.Where(r => r.Job.Salary > 3000));

            //update employee.Birth
            foreach (var entry in list) {
                entry.Birth = entry.Birth.AddYears(-1);
                jobRepo.Update(entry.Job);
            }

            context.Commit();
            context.Dispose();
        }

        private static void MongoBasicCrud() {
            var conStr = ConfigurationManager.ConnectionStrings["PubsMongo"].ConnectionString;
            var context = new MongoRepositoryContext(conStr);

            //Delete from job
            Console.WriteLine("Remove all jobs");
            context.Database.DropCollection<Job>();

            //Fill jobs
            var jobTitles = new[] { "Java", "C", "C++", "Objective-C", "C#", "JavaScript", "PHP", "Python" };
            var jobRepo = new MongoRepository<Job>(context);
            for (int i = 0; i < jobTitles.Length; i++) {
                var job = new Job {
                    Title = jobTitles[i],
                    Salary = Math.Abs(Guid.NewGuid().GetHashCode() % 8000 + 8000),
                };
                jobRepo.Create(job);
            }

            //delete from employee
            Console.WriteLine("Remove all employee");
            context.Database.DropCollection<Employee>();

            //query all jobs
            Console.WriteLine("Query all jobs");
            var jobs = jobRepo.Fetch(q => q.ToList());

            //query part jobs
            var halfJobTitles = jobTitles.Take(jobTitles.Length / 2).ToArray();
            var halfJobs = jobRepo.Retrive(j => j.Title, halfJobTitles);

            //Fill employee
            var employeeNames = "Charles、Mark、Bill、Vincent、William、Joseph、James、Henry、Gary、Martin"
             .Split('、', ' ');
            var employeeRepo = new MongoRepository<Employee>(context);
            for (int i = 0; i < employeeNames.Length; i++) {
                var entry = new Employee {
                    Name = employeeNames[i],
                    Address = Guid.NewGuid().ToString().Substring(0, 8),
                    Birth = DateTime.UtcNow,
                    Job = jobs[Math.Abs(Guid.NewGuid().GetHashCode() % jobs.Count)],
                };
                employeeRepo.Create(entry);
            }

            //query employee
            Console.WriteLine("Query employee where salary > 3000");
            var list = employeeRepo.Fetch(q => q.Where(r => r.Job.Salary > 3000));

            //update employee.Birth
            foreach (var entry in list) {
                entry.Birth = entry.Birth.AddYears(-1);
                jobRepo.Update(entry.Job);
            }

            context.Dispose();
        }

        private static void NHibernateGuidAggregateRoot() {
            var context = new PubsContext();
            context.EventDispatcher.PostLoad += EventDispatcher_PostLoad;
            var deptRepo = new NHibernateRepository<Department, Guid>(context);

            context.Begin();
            var list = deptRepo.All.ToArray();
            foreach (var item in list) {
                deptRepo.Delete(item);
            }

            var deptNames = new[] { "Alfreds Futterkiste", "Ana Trujillo Emparedados y helados",
                "Berglunds snabbk", "Blauer See Delikatessen", "Blondesddsl p",
                "Die Wandernde Kuh", "Drachenblut Delikatessen" };

            foreach (var item in deptNames) {
                deptRepo.Create(new Department {
                    Id = Guid.NewGuid(),
                    Name = item,
                });
            }

            context.Commit();
            context.Dispose();
        }

        private static void EventDispatcher_PostLoad(Object sender, NHibernate.Event.PostLoadEvent e) {
            Console.WriteLine("{0}#{1} loaded", e.Entity, e.Id);
        }

        private static void MongoAggregateRoot() {
            var conStr = ConfigurationManager.ConnectionStrings["PubsMongo"].ConnectionString;
            var context = new MongoRepositoryContext(conStr);
            context.Database.DropCollection<Department>();

            var deptNames = new[] { "Alfreds Futterkiste", "Ana Trujillo Emparedados y helados",
                "Berglunds snabbk", "Blauer See Delikatessen", "Blondesddsl p",
                "Die Wandernde Kuh", "Drachenblut Delikatessen" };

            var deptRepo = new MongoRepository<Department, Guid>(context);
            foreach (var item in deptNames) {
                deptRepo.Create(new Department {
                    Id = Guid.NewGuid(),
                    Name = item,
                });
            }

            context.Database.DropCollection<Shipper>();
            var shipperRepo = new MongoRepository<Shipper, ObjectId>(context);
            var shippers = new List<Shipper>();
            shippers.Add(new Shipper {
                CompanyName = "Speedy Express",
                Phone = "(503) 555-9831",
            });
            shippers.Add(new Shipper {
                CompanyName = "United Package",
                Phone = "(503) 555-3199",
            });
            shippers.Add(new Shipper {
                CompanyName = "Federal Shipping",
                Phone = "(503) 555-9931",
            });
            foreach (var entry in shippers) {
                shipperRepo.Create(entry);
            }

            var shipper2 = shippers[2];
            shipper2.CompanyName += " LC.";
            shipperRepo.Update(shipper2);

            shipperRepo.Delete(shippers[1]);

            var shipper0 = shipperRepo.Retrive(shippers[0].Id);
            Contract.Assert(shippers[0].CompanyName == shipper0.CompanyName);
            Contract.Assert(shippers[0].Phone == shipper0.Phone);
            context.Dispose();
        }

        private static void NHibernate_Dupliate_entity_use_trans() {
            ISessionFactory sessionFactory = PubsContext.DbFactory;
            using (var session = sessionFactory.OpenSession())
            using (session.BeginTransaction()) {
                var job1 = session.Get<Job>(1);
                job1.Title = Guid.NewGuid().ToString();
                session.Update(job1);

                IQueryable<Job> query = new NhQueryable<Job>(session.GetSessionImplementation());
                var job2 = query.FirstOrDefault(r => r.Title == job1.Title);

                //在 using (session.BeginTransaction()) 时返回 true，否则返回 false
                Console.WriteLine("job1 == job2 ? {0}", job1 == job2);

                var list = query.Where(r => r.Id >= 1 && r.Id <= 3).ToList();
                //走缓存
                var job3 = session.Get<Job>(3);
            }
        }

        private static void NHibernate_Dupliate_entity_need_evict_before_update() {
            ISessionFactory sessionFactory = PubsContext.DbFactory;
            using (var session = sessionFactory.OpenSession()) {
                var j1 = session.Get<Job>(1);
                var j2 = new Job {
                    Id = j1.Id,
                    Salary = j1.Salary,
                    Title = j1.Title
                };

                //update j2 失败，先Evict j1
                session.Evict(j1);
                session.Update(j2);
            }

            using (var session = sessionFactory.OpenSession()) {
                IQueryable<Job> query = new NhQueryable<Job>(session.GetSessionImplementation());
                var jobs = query.Where(r => r.Id >= 1).Take(2).ToList();
                var j1 = jobs.First();
                var j2 = new Job {
                    Id = j1.Id,
                    Salary = j1.Salary,
                    Title = j1.Title
                };

                try {
                    session.Update(j2);
                }
                catch (NHibernate.NonUniqueObjectException) {
                    Console.WriteLine("update j2 失败，先Evict j1");
                }

                session.Evict(j1);
                session.Update(j2);
            }

            using (var session = sessionFactory.OpenSession()) {
                IQueryable<Job> query = new NhQueryable<Job>(session.GetSessionImplementation());
                var j1 = query
                    .Where(r => r.Id == 1)
                    .First();
                var j2 = new Job {
                    Id = j1.Id,
                    Salary = 100,
                    Title = j1.Title,
                };

                //update j2 失败，先Evict j1
                session.Evict(j1);
                session.Update(j2);
            }

            using (var session = sessionFactory.OpenSession()) {
                IQueryable<Job> query = new NhQueryable<Job>(session.GetSessionImplementation());
                var jobj = query
                    .Where(r => r.Id == 1)
                    .Select(r => new { r.Id, r.Title })
                    .First();
                var j2 = new Job {
                    Id = jobj.Id,
                    Salary = 100,
                    Title = jobj.Title,
                };

                //不需要 Evict jentry
                session.Update(j2);
            }

            using (var session = sessionFactory.OpenSession()) {
                IQueryable<Job> query = new NhQueryable<Job>(session.GetSessionImplementation());
                var jobj = query
                    .Where(r => r.Id == 1)
                    .Select(r => new {
                        Id = r.Id,
                        Salary = r.Salary,
                        Title = r.Title,
                    })
                    .First();
                var j2 = new Job {
                    Id = jobj.Id,
                    Salary = jobj.Salary,
                    Title = jobj.Title,
                };

                //不需要 Evict jentry
                session.Update(j2);
            }

            using (var session = sessionFactory.OpenSession()) {
                IQueryable<Job> query = new NhQueryable<Job>(session.GetSessionImplementation());
                var jentry = query
                    .Where(r => r.Id == 1)
                    .Select(r => new Job {
                        Id = r.Id,
                        Salary = r.Salary,
                        Title = r.Title,
                    })
                    .First();
                var j2 = new Job {
                    Id = jentry.Id,
                    Salary = jentry.Salary,
                    Title = jentry.Title,
                };

                //不需要 Evict jentry
                session.Update(j2);
            }
        }

        private static void NHibernate_Dupliate_entity_need_evict_and_Statistics() {
            var context = new PubsContext();
            var jobRepo = new NHibernateRepository<Job>(context);

            var j1 = jobRepo.Retrive(1);
            var j2 = jobRepo.Retrive(1);
            Console.WriteLine("j1 == j2? {0}", j1 == j2); // True

            var session = context.EnsureSession();
            Console.WriteLine("before evict, CollectionCount {0}, EntityCount {1}",
                session.Statistics.CollectionCount,
                session.Statistics.EntityCount);

            //j1 == j2, 都指向了Job#1，Evict后是游离态
            session.Evict(j2);
            Console.WriteLine("after  evict, CollectionCount {0}, EntityCount {1}",
                session.Statistics.CollectionCount,
                session.Statistics.EntityCount);

            //Job#1 这个实体并没有任何持久态存在，以下2句完全相同
            session.Update(j1);
            session.Update(j2);

            Console.WriteLine("after update, CollectionCount {0}, EntityCount {1}",
                session.Statistics.CollectionCount,
                session.Statistics.EntityCount);

            var j3 = new Job {
                Id = j1.Id,
                Salary = j2.Salary,
                Title = j2.Title
            };

            try {
                j3.Salary += 1;
                jobRepo.Update(j3); //Failed
                //a different object with the same identifier value was already associated with the session: 1, of entity: Chuye.Persistent.Demo.Job
                Debug.Fail("Should failed for NHibernate.NonUniqueObjectException");
            }
            catch (NHibernate.NonUniqueObjectException ex) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Got NHibernate.NonUniqueObjectException with Entity {0}", ex.EntityName);
                Console.ResetColor();
            }

            Console.WriteLine("before evict, CollectionCount {0}, EntityCount {1}",
                session.Statistics.CollectionCount,
                session.Statistics.EntityCount);
            session.Evict(j1);  //移除j1的游离态，对j3的更新操作才能完成

            Console.WriteLine("after  evict, CollectionCount {0}, EntityCount {1}",
                session.Statistics.CollectionCount,
                session.Statistics.EntityCount);

            jobRepo.Update(j3); //Pass
            Console.WriteLine("j1.Salary: {0}, j3.Salary: {1}", j1.Salary, j3.Salary);
            // j1.Salary: 2760.00000, j3.Salary: 2761.00000

            Console.WriteLine("after update, CollectionCount {0}, EntityCount {1}",
                session.Statistics.CollectionCount,
                session.Statistics.EntityCount);

            Console.WriteLine("j1 == j3? {0}", j1 == j3); // True
            //j1 和 j3 还是不同，但1个处于持久态时，更新另1个必然导致 NonUniqueObjectException 异常
            Console.WriteLine();
        }

    }
}
