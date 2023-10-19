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
using System.Timers;
using System.Windows.Threading;
using System.Windows.Media.Media3D;

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

        private ScreenOverlaySettings screenOverlaySettingsWin;

        /// <summary>
        /// Overlay settings
        /// </summary>
        public bool CodeSettings { get; set; }
        public int XPosCode { get; set; }
        public int YPosCode { get; set; }
        public double FSizeCode { get; set; }
        public string FontCode { get; set; }
        public bool NameSettings { get; set; }
        public int XPosName { get; set; }
        public int YPosName { get; set; }
        public double FSizeName { get; set; }
        public string FontName { get; set; }
        public bool CreatorSettings { get; set; }
        public int XPosCreator { get; set; }
        public int YPosCreator { get; set; }
        public double FSizeCreator { get; set; }
        public string FontCreator { get; set; }
        public bool TimeSettings { get; set; }
        public int XPosTime { get; set; }
        public int YPosTime { get; set; }
        public double FSizeTime { get; set; }
        public string FontTime { get; set; }
        public string OverlaySelection { get; set; }
        public string Default1Entry { get; set; }
        public string Default2Entry { get; set; }


        private LogWindow logWindow;
        private readonly ScreenOverlayWin screenOverlayWin;
        private VirtualCameraOld2 virtualCam;

        private Timer timer;
        private TimeSpan elapsedTime;
        private bool isTimerRunning;


        private bool isResizing = false;

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

            //timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromSeconds(0.1);
            //timer.Tick += Timer_Tick;
            timer = new Timer(1000); // Timer interval in milliseconds (0.1 seconds)
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
        }

        /// <summary>
        /// Loads in all major level information onto the main window
        /// </summary>
        /// <param name="lvl">The most recent active Level object</param>
        public void UpdateActiveLevel(Level lvl)
        {
            ActiveLevel = lvl;
            _activeLevel = lvl;

            if (this.LogAll)
                Utils.CheckExistingLog();

            //
            // TL I can't for the life of me get the binding to update.
            // so set manually for now...
            //
            this.CodeLabel.SetValue(System.Windows.Controls.Label.ContentProperty, lvl.Code);
            this.NameLabel.SetValue(System.Windows.Controls.Label.ContentProperty, lvl.Name);
            this.CreatorLabel.SetValue(System.Windows.Controls.Label.ContentProperty, lvl.Creator);
            this.TransNameLabel.SetValue(System.Windows.Controls.Label.ContentProperty, lvl.Translation);
            this.Deaths.SetValue(System.Windows.Controls.Label.ContentProperty, lvl.DeathCnt);

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
        }

        /// <summary>
        /// Read all locally saved user settings
        /// </summary>
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
                                if (deviceCombo.Items.Contains(appSettings[key]))
                                {
                                    var idx = deviceCombo.Items.IndexOf(appSettings[key]);
                                    deviceCombo.SelectedIndex = idx;
                                    this.Device = appSettings[key];
                                }
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
                            case "WinWidth":
                                Width = Double.Parse(appSettings[key]);
                                break;
                            case "WinHeight":
                                Height = Double.Parse(appSettings[key]);
                                break;
                            case "PlayerVisible":
                                if (bool.Parse(appSettings[key]))
                                    videoPort.Visibility = Visibility.Visible;
                                else
                                    videoPort.Visibility = Visibility.Collapsed;
                                break;
                            default:
                                Type mainWindowType = this.GetType();
                                var propertyInfo = mainWindowType.GetProperty(key); 
                                if (propertyInfo != null && propertyInfo.CanWrite)
                                {
                                    if (int.TryParse(appSettings[key], out int intValue))
                                    {
                                        propertyInfo.SetValue(this, intValue);
                                    }
                                    else if (bool.TryParse(appSettings[key], out bool boolValue))
                                    {
                                        propertyInfo.SetValue(this, boolValue);
                                    }
                                    else
                                    {
                                        propertyInfo.SetValue(this, appSettings[key]);
                                    }
                                }
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
            Utils.Log("Read User Settings");
        }

        static void ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                string result = appSettings[key] ?? "Not Found";
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


            // For catching user window resize event because wpf doesn't
            // have a proper resizeevent that fires AFTER resize is completed
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        // For catching user window resize
        const int WM_SIZING = 0x214;
        const int WM_EXITSIZEMOVE = 0x232;
        private static bool WindowWasResized = false;

        // For catching user window resize
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SIZING)
            {

                if (WindowWasResized == false)
                {

                    //    'indicate the the user is resizing and not moving the window
                    WindowWasResized = true;
                }
            }

            if (msg == WM_EXITSIZEMOVE)
            {

                // 'check that this is the end of resize and not move operation          
                if (WindowWasResized == true)
                {

                    // your stuff to do 
                    Console.WriteLine("End");

                    // 'set it back to false for the next resize/move
                    WindowWasResized = false;

                    //MessageBox.Show(newWidth.ToString() + ", " + newHeight.ToString());
                    //MessageBox.Show("Resized");
                    //saveUserSettings();
                    AddUpdateAppSettings("WinWidth", ActualWidth.ToString());
                    AddUpdateAppSettings("WinHeight", ActualHeight.ToString());

                }
            }

            return IntPtr.Zero;
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

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // The user has initiated a resize action.
            isResizing = true;
            //MessageBox.Show("halps");
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
            Utils.Log("Main Start", true);
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            this.IsRunning = false;
            this.startBtn.IsEnabled = true;
            this.stopBtn.IsEnabled = false;
            ResetTimer();
            Utils.Log("Main Stop", true);
            //this.videoPort.Source = "/LaunchScreen.png";
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
            Utils.Log("User Settings Updated");
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
                bool? result = saveFileDialog.ShowDialog();

                // If the user selects a location, save the copy of the Excel file to that location
                //MessageBox.Show(saveFileDialog.FileName);
                if (result == true && saveFileDialog.FileName != "")
                {
                    string newFilePath = saveFileDialog.FileName;

                    // Make a copy of the original Excel file
                    File.Copy(filePath, newFilePath);

                    // Display a success message
                    //MessageBox.Show("Excel file copied successfully.");
                    this.LogLocation = newFilePath;
                    this.logLocation.Content = newFilePath;
                    saveUserSettings();
                }
                else if (result == false)
                {
                    // User clicked "Cancel" or closed the dialog
                    // So reset the LogAll checkbox
                    //MessageBox.Show("Save operation was canceled.");
                    this.LogAll = false;
                    logAllCB.IsChecked = false;
                }
            }
        }


        private void OpenLvlViewer(object sender, RoutedEventArgs e)
        {
            Utils.OpenLink(ActiveLevel.Link);
        }

        /// <summary>
        /// Allows user to import a previously used Excel log
        /// </summary>
        private void ImportExcelLog(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set the filter and initial directory for the file dialog
            openFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
            openFileDialog.InitialDirectory = "C:\\";

            // Show the dialog and check if the user clicked the "OK" button
            if (openFileDialog.ShowDialog() == true)
            {
                // Get the selected file path
                string filePath = openFileDialog.FileName;

                this.LogLocation = filePath;
                this.logLocation.Content = filePath;
                // Do something with the file path (e.g., open the file)
                // ...

                // Display a message to the user
                //MessageBox.Show("File opened: " + filePath);
            }
        }

        /// <summary>
        /// Open development log
        /// </summary>
        private void OpenDevLog(object sender, RoutedEventArgs e)
        {
            this.logWindow = new LogWindow();
            //var logWindow = new LogWindow();
            var entries = this.LogData.GetAll();
            logWindow.SetLogData(entries);
            logWindow.Show();
        }

        /// <summary>
        /// Timer functions are for users who like to log total 
        /// play time on a particular level so they know when
        /// to move on
        /// </summary>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));
            Dispatcher.Invoke(() => timerTextBlock.Content = elapsedTime.ToString(@"hh\:mm\:ss")); // Update the text on the UI thread
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Utils.Log("Timer Tick");
            elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));
            timerTextBlock.Content = elapsedTime.ToString(@"hh\:mm\:ss"); // Update the text
            // Update your UI elements to display the elapsed time
        }

        public void StartTimer()
        {
            if (!isTimerRunning)
            {
                timer.Start();
                isTimerRunning = true;
            }
        }

        public void PauseTimer()
        {
            if (isTimerRunning)
            {
                timer.Stop();
                isTimerRunning = false;
            }
        }

        public void ResetTimer()
        {
            timer.Stop();
            elapsedTime = TimeSpan.Zero;
            isTimerRunning = false;
            Dispatcher.Invoke(() => timerTextBlock.Content = "00:00:00"); // Reset the text on the UI thread
        }

        /// <summary>
        /// Create new window with a new image RGBA Mat to display relevant information
        /// and be imported into OBS/other streaming software
        /// </summary>
        private void StreamPlay_Click(object sender, RoutedEventArgs e)
        {
            //Open new Open CV Mat window that updates once per second
            //var virtualCam = new VirtualCamera();
            //virtualCam.StartAnimation();

            var scnOverlayWin = new ScreenOverlayWin();
            scnOverlayWin.Show();
        }

        /// <summary>
        /// Function for clearing all level info after user has completed a level or quit
        /// </summary>
        public void ClearLevelInfo()
        {
            ResetTimer();

            // TODO remove all remaining level info
            CodeLabel.Content = "";
            this.NameLabel.Content = "";
            this.CreatorLabel.Content = "";
            this.TransNameLabel.Content = "";
            this.DescLabel.Content = "";
            this.Hearts.Content = "";
            this.Boos.Content = "";
            this.Deaths.Content = "";
            this.ClearRate.Content = "0.0%";
            this.ClearCount.Content = "";
            this.ClearAttempts.Content = "";
        }

        /// <summary>
        /// Open Stream Settings Window
        /// </summary>
        private void StreamSettings_Click(object sender, RoutedEventArgs e)
        {
            screenOverlaySettingsWin = new ScreenOverlaySettings();
            screenOverlaySettingsWin.Show();
        }

        /// <summary>
        /// Handle Video Player Collapse/Hiding
        /// </summary>
        private void CollapseBtnClick(object sender, RoutedEventArgs e)
        {
            //CollapsePlayerBtn.RenderTransform = new RotateTransform(180, 0.5, 0.5);
            rotateTransform.Angle += 180;

            //MessageBox.Show(videoPort.ActualHeight.ToString());
            if (videoPort.Visibility == Visibility.Collapsed)
            {
                // Reset main window height
                Height = 730;
                videoPort.Visibility = Visibility.Visible;
            }
            else
            {
                // Adjust main window height
                Height -= videoPort.ActualHeight;
                videoPort.Visibility = Visibility.Collapsed;
            }

            // Save to user settings
            AddUpdateAppSettings("PlayerVisible", (videoPort.Visibility == Visibility.Visible).ToString());
            AddUpdateAppSettings("WinWidth", ActualWidth.ToString());
            AddUpdateAppSettings("WinHeight", ActualHeight.ToString());
            UpdateLayout();
        }
    }
}
