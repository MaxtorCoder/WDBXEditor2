using DBCD.Providers;
using System.IO;

namespace WDBXEditor2.Controller
{
    public class DBCProvider : IDBCProvider
    {
        public Stream StreamForTableName(string tableName, string build) => File.OpenRead(tableName);
    }
}
