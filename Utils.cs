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

                        // see if user liked or booed and update
                        MessageBox.Show((worksheet.Cells[i, 4].Value != null) + " - " + (lvl.Hearted != null));
                        if (worksheet.Cells[i, 4].Value == null && lvl.Hearted != null)
                        {

                            worksheet.Cells[i, 4].Value = lvl.Hearted;
                        }

                        // Update death count
                        worksheet.Cells[i, 5].Value = int.Parse(worksheet.Cells[i, 5].Value.ToString()) + lvl.DeathCnt;

                        // Update LastPlayed
                        worksheet.Cells[i, 6].Value = lvl.LastPlayed;
                        // Set the cell format to YYYY/M/D HH:MM
                        ExcelRange cell = worksheet.Cells[i, 6];
                        cell.Style.Numberformat.Format = "yyyy/M/d HH:mm";

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
                }

                // Save the changes to the Excel file
                excelPackage.Save();
            }
        }



    }
}
