using DBCD;
using DBCD.Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WDBXEditor2.Controller
{
    public class DBLoader
    {
        public ConcurrentDictionary<string, IDBCDStorage> LoadedDBFiles;

        private readonly DBDProvider dbdProvider;
        private readonly DBCProvider dbcProvider;

        public DBLoader()
        {
            dbdProvider = new DBDProvider();
            dbcProvider = new DBCProvider();

            LoadedDBFiles = new ConcurrentDictionary<string, IDBCDStorage>();
        }

        public void LoadFiles(string[] files)
        {
            var stopWatch = new Stopwatch();
            Parallel.ForEach(files, (file) => 
            {
                stopWatch.Start();
                var dbcd = new DBCD.DBCD(dbcProvider, dbdProvider);
                var storage = dbcd.Load(file, "8.3.0.33941", Locale.EnUS);

                LoadedDBFiles.TryAdd(Path.GetFileName(file), storage);
                stopWatch.Stop();
                Console.WriteLine($"Loading File: {Path.GetFileName(file)} Elapsed Time: {stopWatch.Elapsed}");
            });
        }
    }
}
