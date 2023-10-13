using OfficeOpenXml;
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

namespace MM2Buddy
{
    public delegate void WinFunc();
    /// <summary>
    /// Interaction logic for CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        //public ExcelPackage excelPack;
        //public Action CustomFunction { get; set; }
        public CustomMessageBox(string text)
        {
            InitializeComponent();
            this.MessageText.Text = text;

            Owner = Application.Current.MainWindow;

            // Center the custom message box relative to the owner
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        //private void TryAgainButtonClick(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        excelPack.Save();
        //    }
        //    catch (Exception ex)
        //    {
        //        Utils.Log("Failed Again.  Closing.", true);
        //        MessageBox.Show(ex.Message);
        //    }
                
        //    this.Close();
        //}

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
