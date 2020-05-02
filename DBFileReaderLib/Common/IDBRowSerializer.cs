using System.Collections.Generic;

namespace DBFileReaderLib.Common
{
    interface IDBRowSerializer<T> where T : class
    {
        IDictionary<int, BitWriter> Records { get; }

        void Serialize(IDictionary<int, T> rows);

        void Serialize(int id, T row);

        void GetCopyRows();
    }
}
