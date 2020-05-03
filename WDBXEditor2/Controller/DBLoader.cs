using DBCD;
using DBDefsLib;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using WDBXEditor2.Views;
using static DBDefsLib.Structs;

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
            var dbcd = new DBCD.DBCD(dbcProvider, dbdProvider);
            Stopwatch stopWatch = null;

            foreach (string db2File in files)
            {
                DefinitionSelect definitionSelect = new DefinitionSelect();
                definitionSelect.SetDefinitionFromVersionDefinitions(GetVersionDefinitionsForDB2(db2File));
                definitionSelect.ShowDialog();

                if (definitionSelect.IsCanceled)
                    continue;

                stopWatch = new Stopwatch();
                var storage = dbcd.Load(db2File, definitionSelect.SelectedVersion, definitionSelect.SelectedLocale);
                LoadedDBFiles.TryAdd(Path.GetFileName(db2File), storage);
                stopWatch.Stop();
                Console.WriteLine($"Loading File: {Path.GetFileName(db2File)} Elapsed Time: {stopWatch.Elapsed}");
            }
        }

        public VersionDefinitions[] GetVersionDefinitionsForDB2(string db2File)
        {
            var dbdStream = dbdProvider.StreamForTableName(db2File, null);
            var dbdReader = new DBDReader();
            var databaseDefinition = dbdReader.Read(dbdStream);

            return databaseDefinition.versionDefinitions;
        }
    }
}
