﻿using SpreadsheetGear;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Hec.Dss.Excel;
using Hec.Dss;
using Microsoft.Win32;



namespace DSSExcel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string ExcelFileName { get; set; }
        public string DssFileName { get; set; }
        public RecordType RecordType { get; set; }
        private ExcelReader r;

        public MainWindow()
        {
            InitializeComponent();
            string[] args = Environment.GetCommandLineArgs();
            ExcelFileName = args[1];
            DssFileName = args[2];

            r = new ExcelReader(ExcelFileName);
            DatePage.ExcelView.ActiveWorkbook = r.workbook;
            OrdinatePage.ExcelView.ActiveWorkbook = r.workbook;
            TimeSeriesValuePage.ExcelView.ActiveWorkbook = r.workbook;
            PairedDataValuePage.ExcelView.ActiveWorkbook = r.workbook;

        }

        private void RecordTypePage_PairedDataNextClick(object sender, RoutedEventArgs e)
        {
            RecordTypePage.Visibility = Visibility.Collapsed;
            OrdinatePage.Visibility = Visibility.Visible;
            RecordType = RecordType.PairedData;
        }

        private void RecordTypePage_TimeSeriesNextClick(object sender, RoutedEventArgs e)
        {

            RecordTypePage.Visibility = Visibility.Collapsed;
            DatePage.Visibility = Visibility.Visible;
            RecordType = RecordType.RegularTimeSeries;
        }

        private void DatePage_NextClick(object sender, RoutedEventArgs e)
        {
            //if (!CheckDates(DatePage.Dates))
            //    return;
            DatePage.Visibility = Visibility.Collapsed;
            TimeSeriesValuePage.Visibility = Visibility.Visible;
        }

        private bool CheckDates(IRange dates)
        {
            if (dates.ColumnCount != 1)
            {
                MessageBox.Show("The selection for Date/Time should only have one column of data.", "Date/Time Selection Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            DatePage.ExcelView.ActiveWorkbookSet.GetLock();

            if (!ExcelReader.IsDateRange(dates))
            {
                MessageBox.Show("All values selected for Date/Time don't follow the date and time format.", "Date/Time Selection Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                DatePage.ExcelView.ActiveWorkbookSet.ReleaseLock();
                return false;
            }

            DatePage.ExcelView.ActiveWorkbookSet.ReleaseLock();
            return true;
        }

        private void DatePage_BackClick(object sender, RoutedEventArgs e)
        {
            DatePage.Visibility = Visibility.Collapsed;
            RecordTypePage.Visibility = Visibility.Visible;
        }

        private void OrdinatePage_NextClick(object sender, RoutedEventArgs e)
        {
            if (!CheckOrdinates(OrdinatePage.Ordinates))
                return;
            OrdinatePage.Visibility = Visibility.Collapsed;
            PairedDataValuePage.Visibility = Visibility.Visible;
        }

        private bool CheckOrdinates(IRange ordinates)
        {
            if (ordinates.ColumnCount != 1)
            {
                MessageBox.Show("The selection for ordinates should only have one column of data.", "Ordinate Selection Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            OrdinatePage.ExcelView.ActiveWorkbookSet.GetLock();
            if (!ExcelReader.IsOrdinateRange(ordinates))
            {
                MessageBox.Show("All selected ordinates must be numbers.", "Ordinate Selection Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                OrdinatePage.ExcelView.ActiveWorkbookSet.ReleaseLock();
                return false;
            }


            OrdinatePage.ExcelView.ActiveWorkbookSet.ReleaseLock();
            return true;
        }

        private void OrdinatePage_BackClick(object sender, RoutedEventArgs e)
        {
            OrdinatePage.Visibility = Visibility.Collapsed;
            RecordTypePage.Visibility = Visibility.Visible;
        }

        private void TimeSeriesValuePage_NextClick(object sender, RoutedEventArgs e)
        {
            //if (!CheckTimeSeriesValues(TimeSeriesValuePage.Values))
            //    return;
            TimeSeriesValuePage.Visibility = Visibility.Collapsed;
            ReviewPage.PreviousPage = TimeSeriesValuePage;
            DatePage.ExcelView.ActiveWorkbookSet.GetLock();
            TimeSeriesValuePage.ExcelView.ActiveWorkbookSet.GetLock();
            ReviewPage.SetupReviewPage(RecordType.RegularTimeSeries, DatePage.Dates, TimeSeriesValuePage.Values);
            DatePage.ExcelView.ActiveWorkbookSet.ReleaseLock();
            TimeSeriesValuePage.ExcelView.ActiveWorkbookSet.ReleaseLock();
            ReviewPage.Visibility = Visibility.Visible;
        }

        private bool CheckTimeSeriesValues(IRange values)
        {
            TimeSeriesValuePage.ExcelView.ActiveWorkbookSet.GetLock();
            if (!ExcelReader.IsValuesRange(values))
            {
                MessageBox.Show("All selected values must be numbers.", "Value Selection Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TimeSeriesValuePage.ExcelView.ActiveWorkbookSet.ReleaseLock();
                return false;
            }

            if (values.RowCount != DatePage.Dates.RowCount)
            {
                MessageBox.Show("The row count of selected values must match the row count of selected date/times.", "Value Selection Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TimeSeriesValuePage.ExcelView.ActiveWorkbookSet.ReleaseLock();

                return false;
            }

            TimeSeriesValuePage.ExcelView.ActiveWorkbookSet.ReleaseLock();
            return true;
        }

        private void TimeSeriesValuePage_BackClick(object sender, RoutedEventArgs e)
        {
            TimeSeriesValuePage.Visibility = Visibility.Collapsed;
            DatePage.Visibility = Visibility.Visible;
        }

        private void PairedDataValuePage_NextClick(object sender, RoutedEventArgs e)
        {
            if (!CheckPairedDataValues(PairedDataValuePage.Values))
                return;

            PairedDataValuePage.Visibility = Visibility.Collapsed;
            ReviewPage.PreviousPage = PairedDataValuePage;
            OrdinatePage.ExcelView.ActiveWorkbookSet.GetLock();
            PairedDataValuePage.ExcelView.ActiveWorkbookSet.GetLock();
            ReviewPage.SetupReviewPage(RecordType.PairedData, OrdinatePage.Ordinates, PairedDataValuePage.Values);
            OrdinatePage.ExcelView.ActiveWorkbookSet.ReleaseLock();
            PairedDataValuePage.ExcelView.ActiveWorkbookSet.ReleaseLock();
            ReviewPage.Visibility = Visibility.Visible;
        }

        private bool CheckPairedDataValues(IRange values)
        {
            PairedDataValuePage.ExcelView.ActiveWorkbookSet.GetLock();

            if (!ExcelReader.IsValuesRange(values))
            {
                MessageBox.Show("All selected values must be numbers.", "Value Selection Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                PairedDataValuePage.ExcelView.ActiveWorkbookSet.ReleaseLock();

                return false;
            }

            if (values.RowCount != OrdinatePage.Ordinates.RowCount)
            {
                MessageBox.Show("The row count of selected values must match the row count of selected ordinates.", "Value Selection Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                PairedDataValuePage.ExcelView.ActiveWorkbookSet.ReleaseLock();

                return false;
            }

            PairedDataValuePage.ExcelView.ActiveWorkbookSet.ReleaseLock();
            return true;
        }

        private void PairedDataValuePage_BackClick(object sender, RoutedEventArgs e)
        {
            PairedDataValuePage.Visibility = Visibility.Collapsed;
            OrdinatePage.Visibility = Visibility.Visible;
        }

        private void ReviewPage_ImportClick(object sender, RoutedEventArgs e)
        {
            ExcelReader reader = new ExcelReader(ReviewPage.ExcelView.ActiveWorkbook);
            
            reader.workbookSet.GetLock();
            ReviewPage.ExcelView.ActiveWorkbookSet.GetLock();
            reader.SetActiveSheetInfo(ReviewPage.ExcelView.ActiveWorksheet.Name);
            if (!IsProperlyFormatted(reader))
                return;

            if (ReviewPage.currentRecordType == RecordType.RegularTimeSeries || ReviewPage.currentRecordType == RecordType.IrregularTimeSeries)
                ImportTimeSeries(reader);
            if (ReviewPage.currentRecordType == RecordType.PairedData)
                ImportPairedData(reader);
            reader.workbookSet.ReleaseLock();
            ReviewPage.ExcelView.ActiveWorkbookSet.ReleaseLock();
        }

        private bool IsProperlyFormatted(ExcelReader reader)
        {
            if (!reader.AllPathsAreProper(reader.workbook.ActiveWorksheet.Name, RecordType))
            {
                MessageBox.Show("Not all paths are properly formatted.", "Error: Formatting", MessageBoxButton.OK, MessageBoxImage.Error);
                ReviewPage.ExcelView.ActiveWorkbookSet.ReleaseLock();
                return false;
            }
            else if (!reader.IsAllColumnRowCountsEqual(ReviewPage.ExcelView.ActiveWorksheet.Name))
            {
                MessageBox.Show("The sheet being imported doesn't have proper formatting. Not all columns have the same number of values.", "Error: Formatting", MessageBoxButton.OK, MessageBoxImage.Error);
                ReviewPage.ExcelView.ActiveWorkbookSet.ReleaseLock();
                return false;
            }

            return true;
        }

        private void ImportPairedData(ExcelReader reader)
        {
            PairedData pd = reader.GetPairedData(reader.workbook.ActiveWorksheet.Name);
            WriteRecord(pd);
        }

        private void ImportTimeSeries(ExcelReader reader)
        {
            List<TimeSeries> ts = reader.GetMultipleTimeSeries(reader.workbook.ActiveWorksheet.Name).ToList();
            WriteRecords(ts);
        }

        private void WriteRecords(IEnumerable<TimeSeries> records)
        {
            if (DssFileName.Equals(""))
            {
                SaveFileDialog openFileDialog = new SaveFileDialog();
                openFileDialog.Filter = "DSS Files (*.dss)|*.dss";
                openFileDialog.OverwritePrompt = false;
                if (openFileDialog.ShowDialog() == true)
                    DssFileName = openFileDialog.FileName;
                else
                    return;
            }

            using (DssWriter w = new DssWriter(DssFileName))
            {
                foreach (var record in records)
                    w.Write(record);
            }
            DisplayImportStatus(DssFileName);

        }

        private void WriteRecord(object record)
        {
            if (DssFileName.Equals(""))
            {
                SaveFileDialog openFileDialog = new SaveFileDialog();
                openFileDialog.Filter = "DSS Files (*.dss)|*.dss";
                openFileDialog.OverwritePrompt = false;
                if (openFileDialog.ShowDialog() == true)
                    DssFileName = openFileDialog.FileName;
                else
                    return;
            }

            using (DssWriter w = new DssWriter(DssFileName))
            {
                if (record is TimeSeries)
                    w.Write(record as TimeSeries);
                else if (record is PairedData)
                    w.Write(record as PairedData);
            }
            DisplayImportStatus(DssFileName);
        }

        private void DisplayImportStatus(string filename)
        {
            var r = MessageBox.Show("Import to " + filename + " succeeded. Would you like to import another record?", "Import Success", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (r == MessageBoxResult.Yes)
            {
                ReviewPage.ResetPaths();
                ReviewPage.Visibility = Visibility.Collapsed;
                RecordTypePage.Visibility = Visibility.Visible;
            }
            else
                Close();

        }

        private void ReviewPage_BackClick(object sender, RoutedEventArgs e)
        {
            ReviewPage.Visibility = Visibility.Collapsed;
            ReviewPage.PreviousPage.Visibility = Visibility.Visible;
        }

        private void DatePage_TabSelectionChanged(object sender, EventArgs e)
        {
            ChangeAllActiveExcelTabs(DatePage.ExcelView.ActiveSheet);
        }

        private void OrdinatePage_TabSelectionChanged(object sender, EventArgs e)
        {
            ChangeAllActiveExcelTabs(OrdinatePage.ExcelView.ActiveSheet);
        }

        private void TimeSeriesValuePage_TabSelectionChanged(object sender, EventArgs e)
        {
            ChangeAllActiveExcelTabs(TimeSeriesValuePage.ExcelView.ActiveSheet);
        }

        private void PairedDataValuePage_TabSelectionChanged(object sender, EventArgs e)
        {
            ChangeAllActiveExcelTabs(PairedDataValuePage.ExcelView.ActiveSheet);
        }

        private void ChangeAllActiveExcelTabs(ISheet activeSheet)
        {
            DatePage.ExcelView.ActiveSheet = activeSheet;
            OrdinatePage.ExcelView.ActiveSheet = activeSheet;
            TimeSeriesValuePage.ExcelView.ActiveSheet = activeSheet;
            PairedDataValuePage.ExcelView.ActiveSheet = activeSheet;
        }

        //private void ExcelFileSelectPage_NextClick(object sender, RoutedEventArgs e)
        //{
        //    ExcelFileSelectPage.Visibility = Visibility.Collapsed;
        //    if (ExcelFileSelectPage.NextPage is DateSelectPage)
        //    {
        //        ((DateSelectPage)ExcelFileSelectPage.NextPage).Visibility = Visibility.Visible;
        //    }
        //    else if (ExcelFileSelectPage.NextPage is OrdinateSelectPage)
        //    {
        //        ((OrdinateSelectPage)ExcelFileSelectPage.NextPage).Visibility = Visibility.Visible;
        //    }

        //    r = new ExcelReader(ExcelFileSelectPage.FileName);
        //    DatePage.ExcelView.ActiveWorkbook = r.workbook;
        //    OrdinatePage.ExcelView.ActiveWorkbook = r.workbook;
        //    TimeSeriesValuePage.ExcelView.ActiveWorkbook = r.workbook;
        //    PairedDataValuePage.ExcelView.ActiveWorkbook = r.workbook;
        //}

        //private void ExcelFileSelectPage_BackClick(object sender, RoutedEventArgs e)
        //{
        //    ExcelFileSelectPage.Visibility = Visibility.Collapsed;
        //    RecordTypePage.Visibility = Visibility.Visible;
        //}

    }
}
