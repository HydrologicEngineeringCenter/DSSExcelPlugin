﻿using Hec.Dss;
using Hec.Dss.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using CommandLine;

namespace DSSExcel
{
    public class Options
    {
        [Option('c', "command", Required = false, HelpText = "The only command that will be shown.")]
        public string Command { get; set; }

        [Option('d', "dss-file", Required = false, HelpText = "The source file used for exporting or importing from or to the destination file.")]
        public string DSSFile { get; set; }

        [Option('e', "data-file", Required = false, HelpText = "The destination file where the source file will export or import data.")]
        public string DataFile { get; set; }
    }

    public partial class MainWindow : Window
    {
        public QuickVM GetDataContext
        {
            get { return (QuickVM)DataContext; }
        }
        public MainWindow()
        {
            InitializeComponent();
            if (Environment.GetCommandLineArgs().Length != 0) // If command line args were passed
                Parser.Default.ParseArguments<Options>(Environment.GetCommandLineArgs())
                    .WithParsed(HandleCommandLineArgs);
        }

        private void HandleCommandLineArgs(Options options)
        {
            if (options.DSSFile != "")
            {
                if (File.Exists(options.DSSFile)) // if file exists
                {
                    GetDataContext.DssFilePath = options.DSSFile;
                    GetDataContext.GetAllPaths();
                }
            }
            if (options.DataFile != "")
            {
                if (File.Exists(options.DataFile)) // if file exists
                {
                    GetDataContext.DataFilePath = options.DataFile;
                    GetDataContext.GetAllSheets();
                }
            }
            if (options.Command == "import")
                HideExport();
            if (options.Command == "export")
                HideImport();
        }

        private void HideExport()
        {
            DssPathListHeader.Visibility = Visibility.Collapsed;
            DssPathList.Visibility = Visibility.Collapsed;
            ExportButton.Visibility = Visibility.Collapsed;
            grid.RowDefinitions[3].Height = new GridLength(0);
        }

        private void HideImport()
        {
            DataHeader.Visibility = Visibility.Collapsed;
            SheetList.Visibility = Visibility.Collapsed;
            ImportButton.Visibility = Visibility.Collapsed;
            grid.RowDefinitions[1].Height = new GridLength(0);
        }

        private void DssFileButton_Click(object sender, RoutedEventArgs e)
        {
            GetDssFile();
        }

        private void DataFileButton_Click(object sender, RoutedEventArgs e)
        {
            GetDataFile();
        }

        private bool GetDssFile()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.CreatePrompt = true;
            dialog.Title = "Select or Create DSS File";
            dialog.Filter = "DSS Files (*.dss)|*.dss";
            dialog.OverwritePrompt = false;
            if (dialog.ShowDialog() == true)
            {
                GetDataContext.DssFilePath = dialog.FileName;
                GetDataContext.GetAllPaths();
                return true;
            }
            return false;
        }

        private bool GetDataFile()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.CreatePrompt = true;
            dialog.Title = "Select or Create Excel File";
            dialog.Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|CSV Files (*.csv)|*.csv";
            dialog.OverwritePrompt = false;
            if (dialog.ShowDialog() == true)
            {
                if (!File.Exists(dialog.FileName)) // create data file if it doesn't exist
                {
                    Stream fs = dialog.OpenFile();
                    fs.Close();
                }
                GetDataContext.DataFilePath = dialog.FileName;
                GetDataContext.GetAllSheets();
                return true;
            }
            return false;
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckImportSelections())
                {
                    GetDataContext.SelectedSheets = GetSelectedImportSheets();

                    if (!File.Exists(GetDataContext.DssFilePath))
                        if (!GetDssFile()) { return; }

                    if (!GetDataContext.AreSelectedSheetsRowCountsUniform() && !CanRecordDataBeCut())
                        return;

                    GetDataContext.QuickImport();
                    SheetList.SelectedItems.Clear();
                    DssPathList.SelectedItems.Clear();
                    var result = MessageBox.Show(String.Format("DSS data has successfully been imported from {0} to {1}. Show DSS file in File Explorer?",
                        GetDataContext.DataFilePath, GetDataContext.DssFilePath),
                        "Import Successful", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (result == MessageBoxResult.OK)
                        Process.Start("explorer.exe", @"/select," + Path.GetFullPath(GetDataContext.DssFilePath));
                    
                    
                }
            }
            catch (IOException error)
            {
                MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private bool CanRecordDataBeCut()
        {
            var result = MessageBox.Show("Not all sheets have uniform row counts and data will need to be cut. Is that ok?",
                "Import", MessageBoxButton.YesNo, MessageBoxImage.Information);
            return result == MessageBoxResult.Yes ? true : false;
        }

        private List<string> GetSelectedImportSheets()
        {
            var sheets = new List<string>();
            if (SheetList.SelectedItems.Count != 0)
            {
                for (int i = 0; i < SheetList.SelectedItems.Count; i++)
                    sheets.Add(SheetList.SelectedItems[i].ToString());
            }
            else
            {
                for (int i = 0; i < SheetList.Items.Count; i++)
                    sheets.Add(SheetList.Items[i].ToString());
            }
            return sheets;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckExportSelections())
                {
                    GetDataContext.SelectedPaths = GetSelectedDssPaths();
                    GetDataContext.SelectedSheets = GetExportExcelSheets();

                    if (!File.Exists(GetDataContext.DataFilePath))
                        if (!GetDataFile()) { return; }

                    GetDataContext.QuickExport();
                    SheetList.SelectedItems.Clear();
                    DssPathList.SelectedItems.Clear();
                    var result = MessageBox.Show(String.Format("DSS data has successfully been exported from {0} to {1}. Show data file in File Explorer?",
                        GetDataContext.DssFilePath, GetDataContext.DataFilePath),
                        "Export Successful", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (result == MessageBoxResult.OK)
                        Process.Start("explorer.exe", @"/select," + Path.GetFullPath(GetDataContext.DataFilePath));
                }
            }
            catch (IOException error)
            {
                MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private List<string> GetSelectedDssPaths()
        {
            List<string> paths = new List<string>();
            if (DssPathList.SelectedItems.Count != 0)
            {
                for (int i = 0; i < DssPathList.SelectedItems.Count; i++)
                {
                    paths.Add(new DssPath(DssPathList.SelectedItems[i].ToString()).PathWithoutDate);
                }
            }
            else
            {
                for (int i = 0; i < DssPathList.Items.Count; i++)
                {
                    paths.Add(new DssPath(DssPathList.Items[i].ToString()).PathWithoutDate);
                }
            }
            return paths;
        }

        private List<string> GetExportExcelSheets()
        {
            if (SheetList.SelectedItems.Count != 0)
                return GetSelectedSheets();
            else
                return GenerateExportSheets();
        }

        private List<string> GenerateExportSheets()
        {
            var sheets = new List<string>();

            if (DssPathList.SelectedItems.Count == 0)
            {
                for (int i = 0; i < DssPathList.Items.Count; i++)
                    sheets.Add("SheetImport" + ExcelTools.RandomString(3));
            }
            else
            {
                for (int i = 0; i < DssPathList.SelectedItems.Count; i++)
                    sheets.Add("SheetImport" + ExcelTools.RandomString(3));
            }
            
            return sheets;
        }

        private List<string> GetSelectedSheets()
        {
            var sheets = new List<string>();
            for (int i = 0; i < SheetList.SelectedItems.Count; i++)
            {
                sheets.Add(SheetList.SelectedItems[i].ToString());
            }
            return sheets;
        }

        private bool CheckExportSelections()
        {
            if (SheetList.SelectedItems.Count != 0 && DssPathList.SelectedItems.Count != 0 &&
                SheetList.SelectedItems.Count != DssPathList.SelectedItems.Count)
            {
                MessageBox.Show("The amound of selected sheets and DSS paths do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private bool CheckImportSelections()
        {
            if (DssPathList.SelectedItems.Count != 0 && SheetList.SelectedItems.Count != 0 && 
                DssPathList.SelectedItems.Count != SheetList.SelectedItems.Count)
            {
                MessageBox.Show("The amound of selected sheets and DSS paths do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void ManualImportButton_Click(object sender, RoutedEventArgs e)
        {
            if (!GetDataContext.HasDataFile)
            {
                if (!GetDataFile())
                    return;
            }
            ExcelReader er = new ExcelReader(GetDataContext.DataFilePath);
            DSSExcelManualImport s = new DSSExcelManualImport(GetDataContext.DataFilePath, GetDataContext.DssFilePath);
            if (s.ShowDialog() == true) 
            {
                GetDataContext.DssFilePath = s.dss_filename;
                GetDataContext.GetAllPaths(); 
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.hec.usace.army.mil/fwlink/?linkid=46");
        }
    }
}
