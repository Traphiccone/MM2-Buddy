using System;
using System.Collections.Generic;
using System.IO;
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
using System.Configuration;
using System.Linq.Expressions;
using System.Windows.Interop;
using OpenCvSharp;
using static Emgu.CV.Dai.OpenVino;
using System.ComponentModel;
using System.Xml.Linq;
using Microsoft.Win32;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using Emgu.CV;
using OfficeOpenXml;

namespace MM2Buddy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private Image _imageControl;
        public string Device { get; set; }
        public int DeviceIdx { get; set; }
        public bool LvlViewEndless { get; set; }
        public bool LvlViewReport { get; set; }
        public bool LogAll { get; set; }
        public string LogLocation { get; set; }

        public bool IsRunning { get; set; }
        public ScreenState ScreenState { get; set; }
        public ScreenState LastScreenState { get; set; }
        private Level _activeLevel;
        public Level ActiveLevel { get; set; }
        public CircularBuffer<LogEntry> LogData { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Code"));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("code"));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ActiveLevel.Code"));
            ////PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ActiveLevel"));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Level"));
        }
        public MainWindow()
        {
            InitializeComponent();
            deviceCombo.Initialized += DeviceCombo_Initialized;
            Loaded += MainWindow_Loaded;
            DataContext = this;

            ActiveLevel = new Level {};
            _activeLevel = new Level {};
            // Create an image control to display the OpenCV Mat
            //_imageControl = new Image();

            //// Add the image control to the window's content
            //this.Content = _imageControl;
            this.ScreenState = ScreenState.NoScreen;
            this.LastScreenState = ScreenState.NoScreen;
            this.LogData = new CircularBuffer<LogEntry>(900);
            Utils.Log(this.ScreenState.ToString(), true);

        }
        public void UpdateActiveLevel(Level lvl)
        {
            ActiveLevel = lvl;
            _activeLevel = lvl;

            //
            // TL I can't for the life of me get the binding to update.
            // so set manually for now...
            //
            this.CodeLabel.SetValue(System.Windows.Controls.Label.ContentProperty, lvl.Code);
            this.NameLabel.SetValue(System.Windows.Controls.Label.ContentProperty, lvl.Name);
            this.CreatorLabel.SetValue(System.Windows.Controls.Label.ContentProperty, lvl.Creator);

            //this.OnPropertyChanged(nameof(_page))levelInfoGrid;
            //Binding bind = new Binding("ActiveLevel");
            //bind.Source = lvl;
            //bind.Mode = BindingMode.TwoWay;
            //this.SetBinding(Grid.DataContextProperty, bind); // Text block displays "123"

            //DataContext = this;

            // Taken from the xaml
            // Content = "{Binding Code, UpdateSourceTrigger=PropertyChanged, Mode=TwoWay}"
            // Content = "{Binding ActiveLevel.Name}"
            // Content = "{Binding ActiveLevel.Creator}"

            // TODO Read Excel file and pull previous death count/other data.
        }

        public void ReadAllSettings()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                this.IsRunning = false;

                if (appSettings.Count == 0)
                {
                    MessageBox.Show("AppSettings is empty.");
                }
                else
                {
                    foreach (var key in appSettings.AllKeys)
                    {
                        switch (key)
                        {
                            case "VidDevice":
                            {
                                //deviceCombo.SetValue(LeftProperty, "test");
                                //var x = deviceCombo.Items.Contains(appSettings[key]);
                                if (deviceCombo.Items.Contains(appSettings[key]))
                                {
                                    var idx = deviceCombo.Items.IndexOf(appSettings[key]);
                                    deviceCombo.SelectedIndex = idx;
                                    this.Device = appSettings[key];
                                }
                                // code block
                                break;
                            }
                            case "LvlViewEndless":
                                lvlViewEndlessCB.IsChecked = bool.Parse(appSettings[key]);
                                this.LvlViewEndless = bool.Parse(appSettings[key]);
                                break;
                            case "LvlViewReport":
                                lvlViewReportCB.IsChecked = bool.Parse(appSettings[key]);
                                this.LvlViewReport = bool.Parse(appSettings[key]);
                                break;
                            case "LogAll":
                                logAllCB.IsChecked = bool.Parse(appSettings[key]);
                                this.LogAll = bool.Parse(appSettings[key]);
                                break;
                            case "LogLocation":
                                logLocation.Content = appSettings[key];
                                this.LogLocation = appSettings[key];
                                break;
                            default:
                                // code block
                                break;
                        }
                        //MessageBox.Show(key + "  " + appSettings[key]);
                        //Console.WriteLine("Key: {0} Value: {1}", key, appSettings[key]);

                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Error reading app settings");
            }
        }

        static void ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                string result = appSettings[key] ?? "Not Found";
                MessageBox.Show(result);
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Error reading app settings");
            }
        }

        static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Error writing app settings");
            }
        }



        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Test");

            //
            // Load all of user's optional video feeds
            //
            var deviceList = Utils.GetAllConnectedCameras();

            foreach (KeyValuePair<int, string> entry in deviceList)
            {
                deviceCombo.Items.Add(entry.Value);
            }

            // TODO load user settings if available
            ReadAllSettings();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Test");
            //throw new NotImplementedException();

            //if (!this.hasLoaded)
            //{
            //    this.hasLoaded = true;
            //    DirectoryInfo di = new DirectoryInfo("."); // "." is the current dir we are in
            //    FileInfo[] files = di.GetFiles();
            //    List<string> fileNames = new List<string>(files.Length);
            //    foreach (FileInfo fi in files)
            //        fileNames.Add(fi.Name);
            //    this.listBox1.ItemsSource = fileNames;
            //}
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //
            // TODO save window size if user changes the size

            // Get the new size of the window
            //double newWidth = e.NewSize.Width;
            //double newHeight = e.NewSize.Height;

            // Do something with the new size, such as updating the layout of your UI
        }

        private void DeviceCombo_Initialized(object? sender, EventArgs e)
        {
            //Console.WriteLine("Hello, world!");
            throw new NotImplementedException();
        }


        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            this.IsRunning = true;
            this.startBtn.IsEnabled = false;
            this.stopBtn.IsEnabled = true;
            //string filePath = this.LogLocation;

            VideoProcessor.Process();
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            this.IsRunning = false;
            this.startBtn.IsEnabled = true;
            this.stopBtn.IsEnabled = false;
        }

        private void deviceCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //this.set('Device', deviceCombo.SelectedItem.ToString());
            //this.set('device', deviceCombo.SelectedItem.ToString());
            //this.set('Device', deviceCombo.SelectedItem.ToString());
            this.Device = deviceCombo.SelectedItem.ToString();
            this.DeviceIdx = deviceCombo.Items.IndexOf(deviceCombo.SelectedItem);
            saveUserSettings();
        }

        private void saveUserSettings()
        {
            //MessageBox.Show(deviceCombo.SelectedItem.ToString());
            // Save the settings.
            //ConfigurationManager.AppSettings["VidDevice"] = deviceCombo.SelectedItem.ToString();
            AddUpdateAppSettings("VidDevice", deviceCombo.SelectedItem.ToString());
            AddUpdateAppSettings("LvlViewEndless", lvlViewEndlessCB.IsChecked.ToString());
            AddUpdateAppSettings("LvlViewReport", lvlViewReportCB.IsChecked.ToString());
            AddUpdateAppSettings("LogAll", logAllCB.IsChecked.ToString());
            AddUpdateAppSettings("LogLocation", logLocation.Content.ToString());
            //ConfigurationManager.AppSettings["LvlViewEndless"] = textBoxPassword.Text;
            //ConfigurationManager.AppSettings.
        }

        private void lvlViewEndless_Click(object sender, RoutedEventArgs e)
        {
            this.LvlViewEndless = lvlViewEndlessCB.IsChecked ?? false;
            saveUserSettings();
            //lvlViewEndlessCB.
        }

        private void lvlViewReport_Click(object sender, RoutedEventArgs e)
        {
            this.LvlViewReport = lvlViewReportCB.IsChecked ?? false;
            saveUserSettings();
        }

        private void logAll_Click(object sender, RoutedEventArgs e)
        {
            this.LogAll = logAllCB.IsChecked ?? false;
            saveUserSettings();
            if (this.LogAll && this.LogLocation.Length < 3)
            {

                //    // Create an instance of the Excel application
                //Excel.Application excel = new Excel.Application();
                //excel.Visible = false;

                //    // Open the workbook that you want to copy
                string fileName = "MM2LogTemplate.xlsx";
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                //    MessageBox.Show("2");

                // Prompt the user to select a location to save the copy of the Excel file
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                saveFileDialog.Title = "Select A Location For the MM2 Buddy Excel File";
                saveFileDialog.FileName = "MM2Log.xlsx";
                saveFileDialog.ShowDialog();

                // If the user selects a location, save the copy of the Excel file to that location
                if (saveFileDialog.FileName != "")
                {
                    string newFilePath = saveFileDialog.FileName;

                    // Make a copy of the original Excel file
                    File.Copy(filePath, newFilePath);

                    // Display a success message
                    Console.WriteLine("Excel file copied successfully.");
                    this.LogLocation = newFilePath;
                    this.logLocation.Content = newFilePath;
                    saveUserSettings();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //this.Device = "test";
            Utils.OpenLink(ActiveLevel.Link);
        }
    }
}
