﻿using SpreadsheetGear;
using SpreadsheetGear.Windows.Controls;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DSSExcel
{
    /// <summary>
    /// Interaction logic for OrdinateSelectPage.xaml
    /// </summary>
    public partial class OrdinateSelectPage : UserControl
    {
        public IRange Ordinates
        {
            get
            {
                return ExcelView.RangeSelection;
            }
        }
        public OrdinateSelectPage()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler NextClick;
        public event RoutedEventHandler BackClick;
        public event EventHandler TabSelectionChanged;

        private void OrdinateSelectNextButton_Click(object sender, RoutedEventArgs e)
        {
            this.NextClick?.Invoke(this, e);
        }

        private void OrdinateSelectBackButton_Click(object sender, RoutedEventArgs e)
        {
            this.BackClick?.Invoke(this, e);
        }

        private void ExcelView_ActiveTabChanged(object sender, ActiveTabChangedEventArgs e)
        {
            this.TabSelectionChanged?.Invoke(this, e);
        }
    }
}
