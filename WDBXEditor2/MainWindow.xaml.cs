using DBCD;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WDBXEditor2.Controller;

namespace WDBXEditor2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DBLoader dbLoader = new DBLoader();

        public MainWindow()
        {
            InitializeComponent();

            Exit.Click += (e, o) => Close();

            Title = $"WDBXEditor2  -  {Constants.Version}";
        }

        private void OpenDB_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect         = true,
                Filter              = "DB2 Files (*.db2)|*.db2",
                InitialDirectory    = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var files = openFileDialog.FileNames;
                dbLoader.LoadFiles(files);

                foreach (var loadedFile in openFileDialog.FileNames)
                    OpenDBItems.Items.Add(Path.GetFileName(loadedFile));
            }
        }

        private void OpenDBItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Clear DataGrid
            DB2DataGrid.Columns.Clear();
            DB2DataGrid.ItemsSource = new List<string>();

            var selectedItem = (string)OpenDBItems.SelectedItem;
            if (dbLoader.LoadedDBFiles.TryGetValue(selectedItem, out IDBCDStorage storage))
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var data = new DataTable();
                FillColumns(storage, ref data);
                if (storage.Values.Count > 0)
                    FillData(storage, ref data);

                stopWatch.Stop();
                Console.WriteLine($"Populating Grid: {selectedItem} Elapsed Time: {stopWatch.Elapsed}");

                DB2DataGrid.ItemsSource = data.DefaultView;
            }
        }

        private void FillColumns(IDBCDStorage storage, ref DataTable data)
        {
            var firstItem = storage.Values.First();
            for (var i = 0; i < storage.AvailableColumns.Length; ++i)
            {
                var field = firstItem[storage.AvailableColumns[i]];
                // if (field is Array arr)
                // {
                //     for (var j = 0; j < arr.Length; ++j)
                //         data.Columns.Add($"{storage.AvailableColumns[i]}{j}");
                // }
                // else
                    data.Columns.Add(storage.AvailableColumns[i]);
            }
        }

        private void FillData(IDBCDStorage storage, ref DataTable data)
        {
            foreach (var item in storage.Values)
            {
                var row = data.NewRow();
                for (var i = 0; i < storage.AvailableColumns.Length; ++i)
                {
                    var field = item[storage.AvailableColumns[i]];
                    // if (field is Array arr)
                    // {
                    //     for (var j = 0; j < arr.Length; ++j)
                    //         row[i + j] = arr.GetValue(j).ToString();
                    // 
                    //     i += arr.Length;
                    // }
                    // else
                        row[i] = field.ToString();
                }
                data.Rows.Add(row);
            }
        }
    }
}
