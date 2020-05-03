﻿using DBCD;
using DBDefsLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public string[] LoadFiles(string[] files)
        {
            List<string> loadedFiles = new List<string>();
            var dbcd = new DBCD.DBCD(dbcProvider, dbdProvider);
            Stopwatch stopWatch = null;

            foreach (string db2Path in files)
            {
                string db2Name = Path.GetFileName(db2Path);

                DefinitionSelect definitionSelect = new DefinitionSelect();
                definitionSelect.SetDB2Name(db2Name);
                definitionSelect.SetDefinitionFromVersionDefinitions(GetVersionDefinitionsForDB2(db2Path));
                definitionSelect.ShowDialog();

                if (definitionSelect.IsCanceled)
                    continue;

                stopWatch = new Stopwatch();
                var storage = dbcd.Load(db2Path, definitionSelect.SelectedVersion, definitionSelect.SelectedLocale);
                if (LoadedDBFiles.TryAdd(db2Name, storage))
                    loadedFiles.Add(db2Name);

                stopWatch.Stop();
                Console.WriteLine($"Loading File: {db2Name} Elapsed Time: {stopWatch.Elapsed}");
            }

            return loadedFiles.ToArray();
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
