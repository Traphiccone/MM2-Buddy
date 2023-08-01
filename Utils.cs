using DirectShowLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Markup;
using System.Diagnostics;
using OfficeOpenXml;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TesseractOCR.Pix;
using Google.Cloud.Translate.V3;
using Grpc.Core;
using Google.Api.Gax.ResourceNames;
using System.Text.RegularExpressions;
//using System.Management;

namespace MM2Buddy
{
    internal class Utils
    {
        public static Dictionary<int, string> GetAllConnectedCameras()
        {
            DsDevice[] captureDevices;
            var deviceList = new Dictionary<int, string>();

            // Get the set of directshow devices that are video inputs.
            captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            for (int idx = 0; idx < captureDevices.Length; idx++)
            {
                // Do something with the device here...
                deviceList.Add(idx, captureDevices[idx].Name);
                //MessageBox.Show($"Device: {captureDevices[idx].Name}, Index: {idx}");

            }
            return deviceList;
        }

        public static void OpenLink(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        public static void UpdateLog()
        {
            MainWindow mainWin = (MainWindow)Application.Current.MainWindow;
            if (!mainWin.LogAll)
                return;
            Level lvl = mainWin.ActiveLevel;
            if (lvl.Code == "No Level Detected")
                return;

            // Load the Excel file into an ExcelPackage object
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(mainWin.LogLocation)))
            {
                // Get the first worksheet of the Excel file
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[0];

                // Loop through all rows in the first column to find a matching code
                int lastRow = worksheet.Dimension.End.Row;
                bool rowFound = false;
                for (int i = 1; i <= lastRow; i++)
                {
                    if (worksheet.Cells[i, 1].Value != null && worksheet.Cells[i, 1].Value.ToString() == lvl.Code)
                    {
                        //
                        // Found row.  Update info
                        //

                        // see if user liked or booed and update
                        //MessageBox.Show((worksheet.Cells[i, 4].Value != null) + " - " + (lvl.Hearted != null));
                        if (worksheet.Cells[i, 4].Value == null && lvl.Hearted != null)
                        {

                            worksheet.Cells[i, 4].Value = lvl.Hearted;
                        }

                        // Update death count
                        worksheet.Cells[i, 5].Value = /*int.Parse(worksheet.Cells[i, 5].Value.ToString()) +*/ lvl.DeathCnt;

                        // Update LastPlayed
                        worksheet.Cells[i, 6].Value = lvl.LastPlayed;
                        // Set the cell format to YYYY/M/D HH:MM
                        ExcelRange cell = worksheet.Cells[i, 6];
                        cell.Style.Numberformat.Format = "yyyy/M/d HH:mm";

                        if (/*worksheet.Cells[3, 8].Value != null &&*/ lvl.RecordTime > new TimeSpan(0, 0, 0, 000))
                        {
                            // Set the record time value to a cell
                            worksheet.Cells[3, 8].Value = lvl.RecordTime.ToString(@"mm\:ss\.fff");
                            // Format the cell as a time value
                            worksheet.Cells[3, 8].Style.Numberformat.Format = "mm:ss.000";
                        }
                        if (worksheet.Cells[i, 9].Value == null && lvl.Translation != null)
                        {
                            worksheet.Cells[i, 9].Value = lvl.Translation;
                        }

                        //worksheet.Cells[i, 2].Value = 9;
                        rowFound = true;
                        break;
                    }
                }

                // If the row is not found, insert a new row with level info in row 3
                if (!rowFound)
                {
                    worksheet.InsertRow(3, 1);
                    worksheet.Cells[3, 1].Value = lvl.Code;
                    worksheet.Cells[3, 2].Value = lvl.Name;
                    worksheet.Cells[3, 3].Value = lvl.Creator;
                    worksheet.Cells[3, 4].Value = lvl.Hearted;
                    worksheet.Cells[3, 5].Value = lvl.DeathCnt;

                    worksheet.Cells[3, 6].Value = lvl.LastPlayed;
                    worksheet.Cells[3, 7].Value = lvl.FirstPlayed;
                    // Set the cell format to YYYY/M/D HH:MM
                    ExcelRange lastPlay = worksheet.Cells[3, 6];
                    lastPlay.Style.Numberformat.Format = "yyyy/M/d HH:mm";

                    ExcelRange firstPlay = worksheet.Cells[3, 7];
                    firstPlay.Style.Numberformat.Format = "yyyy/M/d HH:mm";

                    if (lvl.RecordTime > new TimeSpan(0, 0, 0, 000))
                    {
                        // Set the record time value to a cell
                        worksheet.Cells[3, 8].Value = lvl.RecordTime.ToString(@"mm\:ss\.fff");
                        // Format the cell as a time value
                        worksheet.Cells[3, 8].Style.Numberformat.Format = "mm:ss.000";
                    }
                    worksheet.Cells[3, 9].Value = lvl.Translation;
                }

                // Save the changes to the Excel file
                excelPackage.Save();
            }
        }

        public static void CheckExistingLog()
        {
            MainWindow mainWin = (MainWindow)Application.Current.MainWindow;
            Level lvl = mainWin.ActiveLevel;

            // Load the Excel file into an ExcelPackage object
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(mainWin.LogLocation)))
            {
                // Get the first worksheet of the Excel file
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[0];

                // Loop through all rows in the first column to find a matching code
                int lastRow = worksheet.Dimension.End.Row;
                bool rowFound = false;
                for (int i = 1; i <= lastRow; i++)
                {
                    if (worksheet.Cells[i, 1].Value != null && worksheet.Cells[i, 1].Value.ToString() == lvl.Code)
                    {
                        //
                        // Found row.  Update info
                        //

                        // Update death count
                        lvl.DeathCnt = int.Parse(worksheet.Cells[i, 5].Value.ToString());
                        mainWin.Deaths.Content = lvl.DeathCnt;
                        //// Update LastPlayed
                        //worksheet.Cells[i, 6].Value = lvl.LastPlayed;
                        //// Set the cell format to YYYY/M/D HH:MM
                        //ExcelRange cell = worksheet.Cells[i, 6];
                        //cell.Style.Numberformat.Format = "yyyy/M/d HH:mm";

                        //if (worksheet.Cells[3, 8].Value != null && lvl.RecordTime > new TimeSpan(0, 0, 0, 000))
                        //{
                        //    // Set the record time value to a cell
                        //    worksheet.Cells[3, 8].Value = lvl.RecordTime.ToString(@"mm\:ss\.fff");
                        //    // Format the cell as a time value
                        //    worksheet.Cells[3, 8].Style.Numberformat.Format = "mm:ss.000";
                        //}
                        ////worksheet.Cells[i, 2].Value = 9;
                        //rowFound = true;
                        //break;
                    }
                }
            }
        }

        public static void GrabMM2Info()
        {
            //// Create an instance of HttpClient
            //using var client = new HttpClient();

            //// Set the URL of the API endpoint you want to request
            //string url = "https://smm2.info/api/level_info.php/?code=4HR-MJN-7RG";

            //// Create a new HttpRequestMessage with the HTTP method and URL
            //var request = new HttpRequestMessage(HttpMethod.Get, url);

            //// Send the HTTP request without waiting for the response
            //var task = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            ////task.RunSynchronously();
            MainWindow mainWin = (MainWindow)Application.Current.MainWindow;
            //mainWin.HBGrid.Visibility = Visibility.Hidden;

            //mainWin.ActiveLevel.InfoTask = task;
            //mainWin.ActiveLevel.HttpClient = client;

            // Continue with other work while the HTTP request is being sent

            Task<string> task = GetDataAsync(mainWin.ActiveLevel.Code);
            mainWin.ActiveLevel.InfoTask = task;
            Utils.Log("Ping to MM2.info Attempt", true);
            //task.Wait();

            // Get the result of the async method
            //string result = task.Result;

            //// Do something with the result
            //Console.WriteLine(result);

        }
        public static void GrabTranslation()
        {
            MainWindow mainWin = (MainWindow)Application.Current.MainWindow;

            Task<string> task = TranslateJapaneseToEnglish(mainWin.ActiveLevel.Name);
            mainWin.ActiveLevel.TransTask = task;
            Utils.Log("Ping to Google Translate Attempt", true);
            //task.Wait();

            // Get the result of the async method
            //string result = task.Result;

            //// Do something with the result
            //Console.WriteLine(result);

        }
        public static void HandleResponse()
        {
            MainWindow mainWin = (MainWindow)Application.Current.MainWindow;

            //HttpResponseMessage response = mainWin.ActiveLevel.InfoTask.Result;

            // Read the content of the response as a string
            //string json = response.Content.ReadAsStringAsync().Result;
            string json = mainWin.ActiveLevel.InfoTask.Result;
            if (!json.Contains("world_record"))
            {

                //mainWin.HBGrid.Visibility = Visibility.Hidden;
                Utils.Log("SMM2.info ping failed", true);
                mainWin.ActiveLevel.SMM2InfoSuccess = 4;
                mainWin.ActiveLevel.InfoTask = null;
                return;
            }
            mainWin.ActiveLevel.SMM2InfoSuccess = 3;
            Utils.Log("SMM2.info ping success", true);

            JObject obj = JObject.Parse(json);

            JArray array = new JArray();
            array.Add(obj["name"]);
            array.Add(obj["description"]);
            array.Add(obj["world_record"]);
            array.Add(obj["likes"]);
            array.Add(obj["boos"]);
            array.Add(obj["clear_rate_pretty"]);
            array.Add(obj["clears"]);
            array.Add(obj["attempts"]);
            array.Add(obj["uploader"]["name"]);

            string newArrayJson = array.ToString();

            // Deserialize the JSON string into a list of MyObject objects
            List<string> data = JsonConvert.DeserializeObject<List<string>>(newArrayJson);

            //// Do something with the data, such as display it in a UI control
            ///
            mainWin.Hearts.Content = data[3];
            mainWin.Boos.Content = data[4];
            mainWin.ClearRate.Content = data[5];
            mainWin.ClearCount.Content = data[6];
            mainWin.ClearAttempts.Content = data[7];
            mainWin.HBGrid.Visibility = Visibility.Visible;

            if (mainWin.CreatorLabel.Content == "")
            {
                mainWin.CreatorLabel.Content = data[8];
            }

            //foreach (string str in data)
            //{
            //    //MessageBox.Show(str);
            //    //mainWin.Hearts.Content = data["boos"];
            //    //Console.WriteLine(obj.Property3);
            //}

            //// Create an instance of HttpClient
            //using var client = new HttpClient();

            //// Set the URL of the API endpoint you want to request
            //string url = "https://smm2.info/api/level_info.php/?code=4HR-MJN-7RG";

            //// Create a new HttpRequestMessage with the HTTP method and URL
            //var request = new HttpRequestMessage(HttpMethod.Get, url);

            //// Send the HTTP request without waiting for the response
            //var task = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            ////task.RunSynchronously();
            //MainWindow mainWin = (MainWindow)Application.Current.MainWindow;
            ////mainWin.ActiveLevel.InfoTask = task;
            ////mainWin.ActiveLevel.HttpClient = client;

            //// Continue with other work while the HTTP request is being sent

            //Task<string> task = GetDataAsync();
            //mainWin.ActiveLevel.InfoTask = task;
            //task.Wait();

            // Get the result of the async method
            //string result = task.Result;

            //// Do something with the result
            //Console.WriteLine(result);

        }
        public static void HandleTransResponse()
        {
            MainWindow mainWin = (MainWindow)Application.Current.MainWindow;

            //HttpResponseMessage response = mainWin.ActiveLevel.InfoTask.Result;

            // Read the content of the response as a string
            try
            {
                string json = mainWin.ActiveLevel.TransTask.Result;

                if (json.Contains("null"))
                {
                    //mainWin.HBGrid.Visibility = Visibility.Hidden;
                    Utils.Log("Google Translate ping failed", true);
                    mainWin.ActiveLevel.GoogleTransSuccess = 4;
                    mainWin.TransNameLabel.Content = "<request failed>";
                    mainWin.ActiveLevel.TransTask = null;
                    return;
                }
                mainWin.ActiveLevel.Translation = json;
                mainWin.TransNameLabel.Content = json;
                mainWin.ActiveLevel.GoogleTransSuccess = 3;
                Utils.Log("Google Translate ping success", true);
                if (mainWin.ActiveLevel.Active)
                    Utils.UpdateLog();
            }
            catch (Exception ex)
            {
                Utils.Log(ex.ToString());
                mainWin.TransNameLabel.Content = "<request failed>";
                mainWin.ActiveLevel.GoogleTransSuccess = 4;
                mainWin.ActiveLevel.TransTask = null;
                return;
            }
        }

        public static async Task<string> GetDataAsync(string code)
        {
            // Make an HTTP request and get the response
            HttpClient client = new HttpClient();
            //HttpResponseMessage response = await client.GetAsync("https://smm2.info/api/level_info.php/?code=" + code);
            HttpResponseMessage response = await client.GetAsync("https://smm2.wizul.us/mm2/level_info/" + code.Replace("-", ""));

            // Read the content of the response as a string
            string content = await response.Content.ReadAsStringAsync();

            // Return the content as the result of the task
            return content;
        }

        public static void Log(string txt, bool showStatus = false)
        {
            MainWindow mainWin = (MainWindow)Application.Current.MainWindow;

            mainWin.LogData.Add(new LogEntry { Timestamp = DateTime.Now, Message = txt });
            if (showStatus)
            {
                mainWin.StatusLabel.Content = txt;
            }
        }

        public static async Task<string> TranslateJapaneseToEnglish(string japaneseText)
        {
            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "quixotic-tesla-186114-b70a8508470d.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filePath);

            TranslationServiceClient translationClient = await TranslationServiceClient.CreateAsync();

            TranslateTextRequest request = new TranslateTextRequest
            {
                Contents = { japaneseText },
                TargetLanguageCode = "en",
                Parent = new LocationName("quixotic-tesla-186114", "us-central1").ToString()
            };
            TranslateTextResponse response = await translationClient.TranslateTextAsync(request);

            string englishTranslation = response.Translations[0].TranslatedText;
            return englishTranslation;
        }

        public static bool ContainsJapanChar(string input)
        {
            // Define the Unicode character ranges for Japanese script
            //string japanesePattern = @"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}\p{IsCJKSymbolsAndPunctuation}\p{IsCJKCompatibility}\p{IsCJKUnifiedIdeographsExtensionA}\p{IsCJKUnifiedIdeographsExtensionB}]";
            //string japanesePattern = @"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}\p{IsCJKSymbolsAndPunctuation}\p{IsCJKCompatibility}\p{IsCJKUnifiedIdeographsExtensionA}\p{IsCJKUnifiedIdeographsExtensionB}]";
            string japanesePattern = @"[\u3040-\u309F\u30A0-\u30FF\u4E00-\u9FAF\u3000-\u303F\uFF00-\uFFEF]";


            // Create a regular expression pattern to match Japanese characters
            Regex regex = new Regex(japanesePattern);

            // Check if the input string contains any Japanese characters
            return regex.IsMatch(input);
        }


    }
}
