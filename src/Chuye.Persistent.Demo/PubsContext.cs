using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chuye.Persistent.NH;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.ConfigurationSchema;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Event.Default;
using NHibernate.Event;

namespace Chuye.Persistent.Demo {
    class PubsContext : NHibernateRepositoryContext {
        private static readonly ISessionFactory _dbFactory;
        private static readonly EventDispatcher _eventDispatcher;

        public IEventDispatcher EventDispatcher {
            get { return _eventDispatcher; }
        }

        public static ISessionFactory DbFactory {
            get { return _dbFactory; }
        }

        static PubsContext() {
            _eventDispatcher = new EventDispatcher();
            _dbFactory = BuildSessionFactory();
        }

        public PubsContext()
            : base(_dbFactory) {
        }

        private static ISessionFactory BuildSessionFactory() {
            var dbConStr = System.Configuration.ConfigurationManager.ConnectionStrings["PubsMysql"].ConnectionString;
            var dbFluentConfig = Fluently.Configure()
                   .Database(MySQLConfiguration.Standard.ConnectionString(dbConStr))
                   .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Program>());
            var dbConfig = dbFluentConfig.BuildConfiguration();
            dbConfig.SetInterceptor(new NHibernateInterceptor());

            //尝试添加 PostLoadEventListener
            var listeners = dbConfig.EventListeners.PostLoadEventListeners.ToList();
            listeners.Add(_eventDispatcher.PostLoadEventListener);
            dbConfig.EventListeners.PostLoadEventListeners = listeners.ToArray();

            return dbConfig.BuildSessionFactory();
        }
    }

    interface IEventDispatcher {
        event EventHandler<PostLoadEvent> PostLoad;
    }

    class EventDispatcher : IEventDispatcher {
        public event EventHandler<PostLoadEvent> PostLoad;

        public readonly IPostLoadEventListener PostLoadEventListener;

        public EventDispatcher() {
            PostLoadEventListener = new CustomPostLoadEventListener(this);
        }

        private void OnPostLoad(PostLoadEvent @event) {
            if (PostLoad != null) {
                PostLoad(this, @event);
            }
        }

        internal class CustomPostLoadEventListener : DefaultPostLoadEventListener {
            private readonly EventDispatcher _eventDispatcher;
            public CustomPostLoadEventListener(EventDispatcher eventDispatcher) {
                _eventDispatcher = eventDispatcher;
            }

            public override void OnPostLoad(PostLoadEvent @event) {
                base.OnPostLoad(@event);
                _eventDispatcher.OnPostLoad(@event);
            }
        }
    }
}
