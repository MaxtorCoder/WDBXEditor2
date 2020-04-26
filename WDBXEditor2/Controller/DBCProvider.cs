using DBCD.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WDBXEditor2.Controller
{
    public class DBCProvider : IDBCProvider
    {
        public Stream StreamForTableName(string tableName, string build) => File.OpenRead(tableName);
    }
}
