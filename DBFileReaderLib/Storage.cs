using System.Collections.Generic;
using System.IO;

namespace DBFileReaderLib
{
    public class Storage<T> : SortedDictionary<int, T> where T : class, new()
    {
        private readonly DBParser reader;

        public Storage(string fileName) : this(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)) { }

        public Storage(Stream stream) : this(new DBParser(stream)) => reader.ClearCache();

        public Storage(DBParser dbReader)
        {
            reader = dbReader;
            reader.PopulateRecords(this);
        }

        #region Methods

        public void Save(string fileName) => reader.WriteRecords(this, fileName);

        public void Save(Stream stream) => reader.WriteRecords(this, stream);

        #endregion
    }
}
