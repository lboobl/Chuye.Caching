using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent {
    public class BulkCopyHelper {
        private String _connectionString;

        public SqlBulkCopyOptions Option { get;private set; }


        public BulkCopyHelper(String connectionString)
            : this(connectionString, SqlBulkCopyOptions.UseInternalTransaction) {
        }

        public BulkCopyHelper(String connectionString, SqlBulkCopyOptions option) {
            _connectionString = connectionString;
            Option = option;
        }

        public DataTable BuildSchemaTable(Object entry) {
            var schemaTable = new DataTable();
            var props = TypeDescriptor.GetProperties(entry).Cast<PropertyDescriptor>();
            foreach (var prop in props) {
                schemaTable.Columns.Add(prop.Name, prop.PropertyType);
            }
            return schemaTable;
        }

        public DataTable BuildSchemaTable<TEntry>() {
            var schemaTable = new DataTable();
            var targetType = typeof(TEntry);
            var props = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props) {
                schemaTable.Columns.Add(prop.Name, prop.PropertyType);
            }
            return schemaTable;
        }

        internal DataTable BuildSchemaTable(IEnumerable<PropertyInfo> props) {
            var schemaTable = new DataTable();
            foreach (var prop in props) {
                schemaTable.Columns.Add(prop.Name, prop.PropertyType);
            }
            return schemaTable;
        }

        public void Insert<TEntry>(String targetTableName, IEnumerable<TEntry> entries) {
            var targetType = typeof(TEntry);
            var props = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var schemaTable = BuildSchemaTable(props);
            foreach (var entry in entries) {
                var row = schemaTable.NewRow();
                row.ItemArray = props.Select(p => p.GetValue(entry, null)).ToArray();
                schemaTable.Rows.Add(row);
            }

            using (var bcp = new SqlBulkCopy(_connectionString)) {
                bcp.DestinationTableName = targetTableName;
                bcp.WriteToServer(schemaTable);
                bcp.Close();
            }
        }
    }
}
