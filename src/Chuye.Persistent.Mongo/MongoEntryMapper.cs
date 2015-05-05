using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Chuye.Persistent.Mongo {

    public interface IMongoEntryMapper {
        String Map(Object entry);
        String Map<TEntry>();
    }

    //Basic mapper
    public class MongoEntryMapper : IMongoEntryMapper {

        public String Map(Object entry) {
            return Map(entry.GetType());
        }

        public String Map<TEntry>() {
            var entryType = typeof(TEntry);
            return Map(entryType);
        }

        public String Map(Type entryType) {
            var document = entryType.GetCustomAttribute<BsonDocumentDeclarationAttribute>(false);
            if (document != null) {
                return document.Document;
            }
            return entryType.Name;
        }
    }

    public static class MongoEntryMapperFactory {
        private static IMongoEntryMapper _entryMapper;

        public static IMongoEntryMapper Mapper {
            get {
                if (_entryMapper == null) {
                    _entryMapper = new MongoEntryMapper();
                }
                return _entryMapper;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                _entryMapper = value;
            }
        }
    }
}
