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
using System.Drawing.Text;
using MM2Buddy.Models;
using System.Xml.Linq;
using System.IO;
using System.Configuration;

namespace MM2Buddy
{
    /// <summary>
    /// Interaction logic for ScreenOverlaySettings.xaml
    /// </summary>
    public partial class ScreenOverlaySettings : Window
    {
        private const string WatermarkText = "Enter a number between 0 and 2000";

        private UserSettings _settings;
        private string _xmlFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "usersettings.xml");
        public ScreenOverlaySettings()
        {
            InitializeComponent();
            PopulateFontComboBox();
            _settings = new UserSettings();
            DataContext = _settings;
            LoadSettings();
        }

        private void numberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        // TextChanged event to validate the entered number range
        private void pos_Changed(object sender, TextChangedEventArgs e)
        {
            //MessageBox.Show(this.ToString());
            //MessageBox.Show(sender.GetType().ToString());
            if (int.TryParse(((TextBox)sender).Text, out int number))
            {
                // If the number is outside the range, limit it to the valid range
                number = Math.Min(2000, Math.Max(0, number));
                ((TextBox)sender).Text = number.ToString();
                ((TextBox)sender).CaretIndex = XPosCode.Text.Length; // Move caret to the end
            }
            else
            {
                // If parsing fails (e.g., empty input), reset the text to 0
                ((TextBox)sender).Text = "0";
            }
        }

        private void numberTextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (int.TryParse(((TextBox)sender).Text, out int number))
            {
                number += e.Delta > 0 ? 1 : -1;
                number = Math.Min(2000, Math.Max(0, number));
                ((TextBox)sender).Text = number.ToString();
                ((TextBox)sender).CaretIndex = ((TextBox)sender).Text.Length;
            }
        }

        private void PopulateFontComboBox()
        {
            InstalledFontCollection fontsCollection = new InstalledFontCollection();

            foreach (System.Drawing.FontFamily fontFamily in fontsCollection.Families)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = fontFamily.Name;
                FontCode.Items.Add(comboBoxItem);
            }
            foreach (System.Drawing.FontFamily fontFamily in fontsCollection.Families)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = fontFamily.Name;
                FontName.Items.Add(comboBoxItem);
            }
            foreach (System.Drawing.FontFamily fontFamily in fontsCollection.Families)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = fontFamily.Name;
                FontCreator.Items.Add(comboBoxItem);
            }
            foreach (System.Drawing.FontFamily fontFamily in fontsCollection.Families)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = fontFamily.Name;
                FontTime.Items.Add(comboBoxItem);
            }
            LoadSettings();
        }

        private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected font from the ComboBox
            if (FontCode.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedFontName = selectedItem.Content.ToString();
                // Use the selectedFontName as desired (e.g., set it as the font of some other UI element)
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(sender.ToString());
        }

        private void LoadSettings()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

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
                            // Checkboxes
                            case "CodeSettings":
                            case "NameSettings":
                            case "CreatorSettings":
                            case "TimeSettings":
                            {
                                CheckBox foundCheckBox = (CheckBox)FindName(key);
                                if (foundCheckBox != null)
                                {
                                    // Convert the loaded value to a boolean
                                    bool isChecked = bool.Parse(appSettings[key]);

                                    // Set the IsChecked property of the checkbox
                                    foundCheckBox.IsChecked = isChecked;
                                }
                                break;
                            }
                            // TextBoxes
                            case "XPosCode":
                            case "YPosCode":
                            case "FSizeCode":

                            case "XPosName":
                            case "YPosName":
                            case "FSizeName":

                            case "XPosCreator":
                            case "YPosCreator":
                            case "FSizeCreator":

                            case "XPosTime":
                            case "YPosTime":
                            case "FSizeTime":
                            {
                                TextBox foundTextBox = (TextBox)FindName(key);
                                if (foundTextBox != null)
                                {
                                    // Load string from config file
                                    string str = (appSettings[key]);

                                    // Set the IsChecked property of the checkbox
                                    foundTextBox.Text = str;
                                }
                                break;
                            }
                            // ComboBoxes
                            case "FontCode":
                            case "FontName":
                            case "FontCreator":
                            case "FontTime":
                            case "OverlaySelection":
                            {
                                ComboBox foundComboBox = (ComboBox)FindName(key);
                                if (foundComboBox != null)
                                {
                                    if (foundComboBox.Items.Contains(appSettings[key]))
                                    {
                                        MessageBox.Show(appSettings[key]);
                                        var idx = foundComboBox.Items.IndexOf(appSettings[key]);
                                        foundComboBox.SelectedIndex = idx;
                                    }
                                    else
                                    {
                                        //MessageBox.Show(key);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("derp");
                                }
                                break;
                            }
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
            Utils.Log("Read User Settings");
            //try
            //{
            //    XDocument xmlDoc = XDocument.Load(_xmlFilePath);

            //    _settings = new UserSettings
            //    {
            //        //UserName = xmlDoc.Element("UserSettings").Element("UserName").Value,
            //        CodeSettings = bool.Parse(xmlDoc.Element("UserSettings").Element("CodeSettings").Value),
            //        //NotificationInterval = int.Parse(xmlDoc.Element("UserSettings").Element("NotificationInterval").Value)
            //    };

            //    DataContext = _settings;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Error loading settings: " + ex.Message);
            //}
        }

        private void ValueChange(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                string checkBoxName = checkBox.Name;
                // Now you can use the checkBoxName variable as the Name of the checkbox.
                UpdateOverlaySettings(checkBoxName, checkBox.IsChecked.ToString());
            }
            else if (sender is TextBox textBox)
            {
                string textBoxName = textBox.Name;
                UpdateOverlaySettings(textBoxName, textBox.Text.ToString());
            }
            else if (sender is ComboBox comboBox)
            {
                if (comboBox.SelectedItem != null)
                {
                    string comboBoxName = comboBox.Name;
                    UpdateOverlaySettings(comboBoxName, ((ComboBoxItem)comboBox.SelectedItem).Content.ToString());
                }
            }
        }

        static void UpdateOverlaySettings(string key, string value)
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
    }
}
