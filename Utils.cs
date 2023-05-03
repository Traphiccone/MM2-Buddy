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



    }
}
