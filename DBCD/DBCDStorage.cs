using DBCD.Helpers;

using DBFileReaderLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace DBCD
{
    public class DBCDRow : DynamicObject
    {
        public int ID;

        private dynamic raw;
        private readonly FieldAccessor fieldAccessor;

        internal DBCDRow(int ID, dynamic raw, FieldAccessor fieldAccessor)
        {
            this.raw = raw;
            this.fieldAccessor = fieldAccessor;
            this.ID = ID;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return fieldAccessor.TryGetMember(raw, binder.Name, out result);
        }

        public object this[string fieldName]
        {
            get => fieldAccessor[raw, fieldName];
        }

        public object this[string filename, string fieldname]
        {
            set
            {
                var newRaw = (object)raw;
                var type = newRaw.GetType().GetField(fieldname);
                type.SetValue(newRaw, Convert.ChangeType(value, type.FieldType));
                raw = (dynamic)newRaw;
            }
        }

        public T Field<T>(string fieldName)
        {
            return (T)fieldAccessor[raw, fieldName];
        }

        public T FieldAs<T>(string fieldName)
        {
            return fieldAccessor.GetMemberAs<T>(raw, fieldName);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return fieldAccessor.FieldNames;
        }
    }

    public class DynamicKeyValuePair<T>
    {
        public T Key;
        public dynamic Value;

        internal DynamicKeyValuePair(T key, dynamic value)
        {
            Key = key;
            Value = value;
        }
    }

    public interface IDBCDStorage : IEnumerable<DynamicKeyValuePair<int>>, IDictionary<int, DBCDRow>
    {
        string[] AvailableColumns { get; }

        DBCDInfo GetDBCDInfo();
        Dictionary<ulong, int> GetEncryptedSections();
        void Save(string filename);
    }

    public class DBCDStorage<T> : Dictionary<int, DBCDRow>, IDBCDStorage where T : class, new()
    {
        private readonly FieldAccessor fieldAccessor;
        private readonly Storage<T> db2Storage;
        private readonly DBCDInfo info;
        private readonly DBParser parser;

        string[] IDBCDStorage.AvailableColumns => info.availableColumns;
        public override string ToString() => $"{info.tableName}";

        public DBCDStorage(Stream stream, DBCDInfo info) : this(new DBParser(stream), info) { }

        public DBCDStorage(DBParser dbReader, DBCDInfo info) : this(dbReader, dbReader.GetRecords<T>(), info) { }

        public DBCDStorage(DBParser parser, Storage<T> storage, DBCDInfo info) : base(new Dictionary<int, DBCDRow>())
        {
            this.info       = info;
            fieldAccessor   = new FieldAccessor(typeof(T), info.availableColumns);
            this.parser     = parser;
            db2Storage      = storage;

            foreach (var record in db2Storage)
                Add(record.Key, new DBCDRow(record.Key, record.Value, fieldAccessor));
        }

        IEnumerator<DynamicKeyValuePair<int>> IEnumerable<DynamicKeyValuePair<int>>.GetEnumerator()
        {
            var enumerator = GetEnumerator();
            while (enumerator.MoveNext())
                yield return new DynamicKeyValuePair<int>(enumerator.Current.Key, enumerator.Current.Value);
        }
        
        public Dictionary<ulong, int> GetEncryptedSections() => parser.GetEncryptedSections();

        public DBCDInfo GetDBCDInfo() => info;

        public void Save(string filename) => db2Storage?.Save(filename);
    }
}
