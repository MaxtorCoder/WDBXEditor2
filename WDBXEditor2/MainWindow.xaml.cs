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
        private string currentOpenDB2 = string.Empty;
        private IDBCDStorage openedDB2Storage;

        public MainWindow()
        {
            InitializeComponent();

            Exit.Click += (e, o) => Close();

            Title = $"WDBXEditor2  -  {Constants.Version}";
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "DB2 Files (*.db2)|*.db2",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
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

            currentOpenDB2 = (string)OpenDBItems.SelectedItem;
            if (currentOpenDB2 == null)
                return;

            if (dbLoader.LoadedDBFiles.TryGetValue(currentOpenDB2, out IDBCDStorage storage))
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var data = new DataTable();
                PopulateColumns(storage, ref data);
                if (storage.Values.Count > 0)
                    PopulateDataView(storage, ref data);

                stopWatch.Stop();
                Console.WriteLine($"Populating Grid: {currentOpenDB2} Elapsed Time: {stopWatch.Elapsed}");

                openedDB2Storage = storage;
                DB2DataGrid.ItemsSource = data.DefaultView;
            }

            Title = $"WDBXEditor2  -  {Constants.Version}  -  {currentOpenDB2}";
        }

        /// <summary>
        /// Populate the DataView with the DB2 Columns.
        /// </summary>
        private void PopulateColumns(IDBCDStorage storage, ref DataTable data)
        {
            var firstItem = storage.Values.First();

            foreach (string columnName in firstItem.GetDynamicMemberNames())
            {
                var columnValue = firstItem[columnName];

                if (columnValue.GetType().IsArray)
                {
                    Array columnValueArray = (Array)columnValue;
                    for (var i = 0; i < columnValueArray.Length; ++i)
                        data.Columns.Add(columnName + i);
                }
                else
                    data.Columns.Add(columnName);
            }
        }

        /// <summary>
        /// Populate the DataView with the DB2 Data.
        /// </summary>
        private void PopulateDataView(IDBCDStorage storage, ref DataTable data)
        {
            foreach (var rowData in storage.Values)
            {
                var row = data.NewRow();

                foreach (string columnName in rowData.GetDynamicMemberNames())
                {
                    var columnValue = rowData[columnName];

                    if (columnValue.GetType().IsArray)
                    {
                        Array columnValueArray = (Array)columnValue;
                        for (var i = 0; i < columnValueArray.Length; ++i)
                            row[columnName + i] = columnValueArray.GetValue(i);
                    }
                    else
                        row[columnName] = columnValue;
                }

                data.Rows.Add(row);
            }
        }

        /// <summary>
        /// Close the currently opened DB2 file.
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Title = $"WDBXEditor2  -  {Constants.Version}";

            // Remove the DB2 file from the open files.
            OpenDBItems.Items.Remove(currentOpenDB2);

            // Clear DataGrid
            DB2DataGrid.Columns.Clear();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (currentOpenDB2 != "" || currentOpenDB2 != string.Empty)
                dbLoader.LoadedDBFiles[currentOpenDB2].Save(currentOpenDB2);
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DB2DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (e.Column != null)
                {
                    var rowIdx = e.Row.GetIndex();
                    if (rowIdx > openedDB2Storage.Keys.Count)
                        throw new Exception();

                    var newVal = e.EditingElement as TextBox;

                    var dbcRow = openedDB2Storage.Values.ElementAt(rowIdx);
                    // dbcRow[e.Column.Header.ToString()] = newVal.Text;

                    Console.WriteLine($"RowIdx: {rowIdx} Text: {newVal.Text}");
                }
            }
        }
    }
}
