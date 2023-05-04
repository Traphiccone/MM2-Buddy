using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using OpenCvSharp;
using Tesseract;
using Emgu.CV;
using Emgu.CV.OCR;
using System.Drawing;
using System.Windows.Xps.Packaging;
using System.Windows.Media.Imaging;
using System.Windows;
using TesseractOCR.Enums;
using TesseractOCR;
using System.Windows.Controls;
using System.Security.Cryptography;
using Emgu.CV.Structure;
using System.Text.RegularExpressions;
//using OpenCvSharpExtern;

namespace MM2Buddy
{
    class VideoProcessor
    {
        public static void Process(/*string[] args*/)
        {
            MainWindow mainWin = (MainWindow)Application.Current.MainWindow;

            var ms = new MemoryStream();
            int idx = mainWin.DeviceIdx;
            int frameWidth = 1920;
            int frameHeight = 1080;

            //var devices = VideoCapture.GetDevices();
            //foreach (var device in devices)
            //{
            //}

            // create a new VideoCapture object with camera index 0 (default camera)


            //var deviceList = Utils.GetAllConnectedCameras();

            //foreach (KeyValuePair<int, string> entry in deviceList)
            //{
            //    //deviceCombo.Items.Add(entry.Value);
            //    MessageBox.Show($"Device: {entry.Value}, Index: {entry.Key}");

            //}

            //
            //
            //
            //OpenCvSharp.VideoCapture capture = new OpenCvSharp.VideoCapture(idx);
            var capture = new OpenCvSharp.VideoCapture(0, VideoCaptureAPIs.DSHOW);

            //MessageBox.Show("Helppppppppppp");
            //MessageBox.Show(capture.FrameHeight.ToString());
            //capture.
            capture.Set(VideoCaptureProperties.FrameWidth, frameWidth);
            capture.Set(VideoCaptureProperties.FrameHeight, frameHeight);

            //MessageBox.Show("Helppppppppppp2");


            // check if the VideoCapture object was successfully created
            if (!capture.IsOpened())
            {
                //Console.WriteLine("Failed to open video device!");
                MessageBox.Show("Failed to open video device!");
                return;
            }

            // create a new window to display the camera feed
            //Cv2.NamedWindow("Webcam");

            // loop through the frames in the camera feed

            while (mainWin.stopBtn.IsEnabled)
            {
                // read a new frame from the camera feed
                OpenCvSharp.Mat frame = new OpenCvSharp.Mat();
                capture.Read(frame);

                // check if the frame was successfully read
                if (frame.Empty())
                {
                    Console.WriteLine("Failed to read frame!");
                    break;
                }

                //OpenCvSharp.Point center = new OpenCvSharp.Point(100, 100);
                //int radius = 30;
                //Scalar color = Scalar.Red;
                //Cv2.Circle(frame, center, radius, color, -1);

                //
                // Check image screen, if one of the optional image screens 
                // is shown, then break it a part and run OCR for each 
                // individual section
                //


                //var str = GetOCRText(frame);
                //OpenCvSharp.Point center = new OpenCvSharp.Point(1290, 935);
                //int radius = 30;
                //Scalar color = Scalar.Red;
                //Cv2.Circle(frame, center, radius, color, -1);


                var bitmapSource = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(frame);
                mainWin.videoPort.Source = bitmapSource;

                var state = CheckScreenState(bitmapSource);
                //MessageBox.Show(state.ToString());


                //
                // If screenState is anything other than NoScreen then start the OCR process
                // Break down the image first to speed up OCR
                //
                if (state != ScreenState.NoScreen)
                {
                    BreakDownImage(frame, state, bitmapSource);
                }

                // display the frame in the "Webcam" window
                //Cv2.ImShow("Large View", frame);

                //MessageBox.Show(frame.Width + "    " + frame.Height);

                // wait for a key press (100ms)
                int key = Cv2.WaitKey(100);

                // check if the "Esc" key was pressed
                if (key == 27)
                {
                    break;
                }
            }

            // release the VideoCapture object and destroy the window
            capture.Release();
            Cv2.DestroyAllWindows();
        }
        //public string GetOCRText(BitmapSource bmap)
        static public string GetOCRText(OpenCvSharp.Mat frame, string type = "text")
        {
            //var test = new TesseractEngine();
            using var img = TesseractOCR.Pix.Image.LoadFromMemory(frame.ToBytes());
            //Pix img = TesseractOCR.Pix.
            //using var img = TesseractOCR.Pix.Image.
            var ocrtext = string.Empty;
            //var engine = new TesseractEngine(@"freeOCR/tessdata", "eng", EngineMode.Default);
            //using var engine = new Engine(@"C:\freeOCR\tessdata", Language.English, TesseractOCR.Enums.EngineMode.Default);
            if (type == "code")
            {
                using (var engine = new Engine(@"C:\Program Files\Tesseract-OCR\tessdata", Language.English, TesseractOCR.Enums.EngineMode.Default))
                {
                    engine.SetVariable("tessedit_char_whitelist", "0123456789QWERTYUPASDFGHJKLXCVBNM-");

                    using (img)
                    {
                        using (var page = engine.Process(img))
                        {
                            ocrtext = page.Text;
                        }
                    }
                }
            }
            else
            {
                using (var engine = new Engine(@"C:\Program Files\Tesseract-OCR\tessdata", Language.English, TesseractOCR.Enums.EngineMode.Default))
                {
                    using (img)
                    //using (var img = Tesseract.Pix.LoadFromMemory(bmap))
                    {
                        using (var page = engine.Process(img))
                        {
                            ocrtext = page.Text;
                        }
                    }
                }
            }


            return ocrtext;
        }
        //static public void CheckScreenState(OpenCvSharp.Mat frame)
        static public ScreenState CheckScreenState(BitmapSource bmap)
        {
            var perMatchAllowed = 97; //97

            var lvlStartScreen = false;
            var lvlPlayedScreen = false;
            var lvlPlayedScreenRpt = false;
            var lvlScreen = false;
            var lvlScreenRpt = false;
            var lvlPopScreen = false;
            var lvlPopScreenRpt = false;

            // Screen when starting a level/100 man
            bool checkLvlStartScreen(BitmapSource bmap)
            {
                //
                // Coordinates and colors that represent the Level Start Screen
                //
                PixelColorCheck p1 = new PixelColorCheck(380, 650, 0, 0, 0); // Left of mario black
                PixelColorCheck p2 = new PixelColorCheck(1600, 650, 0, 0, 0); // Right Mario Black
                PixelColorCheck p3 = new PixelColorCheck(960, 890, 0, 0, 0); // Below Mario Black
                PixelColorCheck p4 = new PixelColorCheck(235, 220, 255, 204, 30); // Left Title Yellow
                PixelColorCheck p5 = new PixelColorCheck(1640, 220, 255, 204, 30); // Right Title Yellow 
                PixelColorCheck p1C = GenerateCompPixel(bmap, p1.X, p1.Y);
                PixelColorCheck p2C = GenerateCompPixel(bmap, p2.X, p2.Y);
                PixelColorCheck p3C = GenerateCompPixel(bmap, p3.X, p3.Y);
                PixelColorCheck p4C = GenerateCompPixel(bmap, p4.X, p4.Y);
                PixelColorCheck p5C = GenerateCompPixel(bmap, p5.X, p5.Y);

                //Vec3b pixel = frame.At<Vec3b>(1290, 935);
                //byte[] pixel = new byte[3];
                //bmap.CopyPixels(new Int32Rect(1695, 580, 1, 1), pixel, 3, 0);


                //MessageBox.Show("Comp Result: " + p1.CompareColor(p1C) + "\n" +
                //    "p1: " + p1.R + ", " + p1.G + ", " + p1.B + "\n" +
                //    "p2: " + p1C.R + ", " + p1C.G + ", " + p1C.B);

                double totalComp = (p1.CompareColor(p1C) + p2.CompareColor(p2C) + p3.CompareColor(p3C) + p4.CompareColor(p4C) + p5.CompareColor(p5C)) / 5;

                //MessageBox.Show("Pixel Colors - > 1: " + pixel[0] + "  2: " + pixel[1] + "  3: " + pixel[2]);
                //if (totalComp > perMatchAllowed)
                //    MessageBox.Show("LvlStart Detected");
                //MessageBox.Show("Total Comp%: " + totalComp);
                return totalComp > perMatchAllowed;
                //PixelColorCheck p1C = new PixelColorCheck(1290, 935, frame.at)
            }
            bool checkLvlPlayedScreen(BitmapSource bmap)
            {
                //
                // Coordinates and colors that represent the Level Played/Liked View Screen
                //
                PixelColorCheck p1 = new PixelColorCheck(1290, 935, 255, 204, 30); // Play button yellow
                PixelColorCheck p2 = new PixelColorCheck(912, 975, 255, 204, 30); // Play Together button yellow
                PixelColorCheck p3 = new PixelColorCheck(718, 462, 253, 252, 238); // White area under tags button yellow
                PixelColorCheck p4 = new PixelColorCheck(713, 333, 101, 29, 29); // Heart icon brown maroon 
                PixelColorCheck p5 = new PixelColorCheck(1695, 580, 213, 212, 200); // area to the far right light grey 
                PixelColorCheck p1C = GenerateCompPixel(bmap, p1.X, p1.Y);
                PixelColorCheck p2C = GenerateCompPixel(bmap, p2.X, p2.Y);
                PixelColorCheck p3C = GenerateCompPixel(bmap, p3.X, p3.Y);
                PixelColorCheck p4C = GenerateCompPixel(bmap, p4.X, p4.Y);
                PixelColorCheck p5C = GenerateCompPixel(bmap, p5.X, p5.Y);

                //Vec3b pixel = frame.At<Vec3b>(1290, 935);
                //byte[] pixel = new byte[3];
                //bmap.CopyPixels(new Int32Rect(1695, 580, 1, 1), pixel, 3, 0);


                //MessageBox.Show("Comp Result: " + p1.CompareColor(p1C) + "\n" +
                //    "p1: " + p1.R + ", " + p1.G + ", " + p1.B + "\n" +
                //    "p2: " + p1C.R + ", " + p1C.G + ", " + p1C.B);

                double totalComp = (p1.CompareColor(p1C) + p2.CompareColor(p2C) + p3.CompareColor(p3C) + p4.CompareColor(p4C) + p5.CompareColor(p5C)) / 5;

                //MessageBox.Show("Pixel Colors - > 1: " + pixel[0] + "  2: " + pixel[1] + "  3: " + pixel[2]);

                //MessageBox.Show("Total Comp%: " + totalComp);
                return totalComp > perMatchAllowed;
                //PixelColorCheck p1C = new PixelColorCheck(1290, 935, frame.at)
            }
            //
            // If Play Screen detected, check for Report Button hover
            //
            bool checkLvlPlayedScreenRpt(BitmapSource bmap)
            {
                //
                // Coordinates for 4 corners of report box
                //
                PixelColorCheck p1 = GenerateCompPixel(bmap, 349, 936);
                PixelColorCheck p2 = GenerateCompPixel(bmap, 528, 936);
                PixelColorCheck p3 = GenerateCompPixel(bmap, 349, 991);
                PixelColorCheck p4 = GenerateCompPixel(bmap, 528, 991);

                //MessageBox.Show("Total Report Comp%: " + totalComp);

                // Check that they are nearly identical and mostly red
                //MessageBox.Show("Report Corner Match: " + (p1.CompareColor(p2) + p2.CompareColor(p3) + p3.CompareColor(p4)) / 3);
                if ((p1.CompareColor(p2) + p2.CompareColor(p3) + p3.CompareColor(p4)) / 3 < perMatchAllowed)
                    return false;

                //MessageBox.Show("Is Red? : " + (p1.R) + " " + (p1.G) + " " + (p1.B));
                //if (p1.R > 200 && p1.G < 150 && p1.B < 150)
                //    MessageBox.Show("LvlPlayedScreenRpt Detected");

                // Check for Red
                return ((p1.R > 200) && (p1.G < 150) && (p1.B < 150));
                //MessageBox.Show("Comp Result: \n" +
                //    "p1: " + p1.R + ", " + p1.G + ", " + p1.B);
            }
            bool checkLvlScreen(BitmapSource bmap)
            {
                //
                // Coordinates and colors that represent the Browsing section Level View Screen
                //
                PixelColorCheck p1 = new PixelColorCheck(1436, 944, 255, 204, 30); // Play button yellow
                PixelColorCheck p2 = new PixelColorCheck(1020, 937, 255, 204, 30); // Play Together button yellow
                PixelColorCheck p3 = new PixelColorCheck(850, 490, 253, 252, 238); // White area under tags button yellow
                PixelColorCheck p4 = new PixelColorCheck(736, 366, 101, 29, 29); // Heart icon brown maroon 
                PixelColorCheck p5 = new PixelColorCheck(1770, 600, 0, 153, 130); // area to the far right teal
                PixelColorCheck p1C = GenerateCompPixel(bmap, p1.X, p1.Y);
                PixelColorCheck p2C = GenerateCompPixel(bmap, p2.X, p2.Y);
                PixelColorCheck p3C = GenerateCompPixel(bmap, p3.X, p3.Y);
                PixelColorCheck p4C = GenerateCompPixel(bmap, p4.X, p4.Y);
                PixelColorCheck p5C = GenerateCompPixel(bmap, p5.X, p5.Y);

                //Vec3b pixel = frame.At<Vec3b>(1290, 935);
                //byte[] pixel = new byte[3];
                //bmap.CopyPixels(new Int32Rect(1695, 580, 1, 1), pixel, 3, 0);


                //MessageBox.Show("1Comp Result: " + p1.CompareColor(p1C) + "\n" +
                //    "p1: " + p1.R + ", " + p1.G + ", " + p1.B + "\n" +
                //    "p2: " + p1C.R + ", " + p1C.G + ", " + p1C.B);

                double totalComp = (p1.CompareColor(p1C) + p2.CompareColor(p2C) + p3.CompareColor(p3C) + p4.CompareColor(p4C) + p5.CompareColor(p5C)) / 5;

                //MessageBox.Show("Pixel Colors - > 1: " + pixel[0] + "  2: " + pixel[1] + "  3: " + pixel[2]);

                //MessageBox.Show("Total LvlScreen Comp%: " + totalComp);
                //if (totalComp > perMatchAllowed)
                //    MessageBox.Show("LvlScreen Detected");
                return totalComp > perMatchAllowed;
                //PixelColorCheck p1C = new PixelColorCheck(1290, 935, frame.at)
            }
            //
            // If Lvl Screen detected, check for Report Button hover
            //
            bool checkLvlScreenRpt(BitmapSource bmap)
            {
                //
                // Coordinates for 4 corners of report box
                //
                PixelColorCheck p1 = GenerateCompPixel(bmap, 370, 970);
                PixelColorCheck p2 = GenerateCompPixel(bmap, 550, 970);
                PixelColorCheck p3 = GenerateCompPixel(bmap, 370, 1026);
                PixelColorCheck p4 = GenerateCompPixel(bmap, 550, 1026);

                //MessageBox.Show("Total Report Comp%: " + totalComp);

                // Check that they are nearly identical and mostly red
                //MessageBox.Show("Report Corner Match: " + (p1.CompareColor(p2) + p2.CompareColor(p3) + p3.CompareColor(p4)) / 3);
                if ((p1.CompareColor(p2) + p2.CompareColor(p3) + p3.CompareColor(p4)) / 3 < perMatchAllowed)
                    return false;

                //MessageBox.Show("Is Red? : " + (p1.R) + " " + (p1.G) + " " + (p1.B));
                //if (p1.R > 200 && p1.G < 150 && p1.B < 150)
                //    MessageBox.Show("LvlScreenRpt Detected");

                // Check for Red
                return ((p1.R > 200) && (p1.G < 150) && (p1.B < 150));
                //MessageBox.Show("Comp Result: \n" +
                //    "p1: " + p1.R + ", " + p1.G + ", " + p1.B);
            }
            bool checkPopLvlScreen(BitmapSource bmap)
            {
                //
                // Coordinates and colors that represent the Browsing section Level View Screen
                //
                PixelColorCheck p1 = new PixelColorCheck(1436, 944, 255, 204, 30); // Play button yellow
                PixelColorCheck p2 = new PixelColorCheck(1020, 937, 255, 204, 30); // Play Together button yellow
                PixelColorCheck p3 = new PixelColorCheck(850, 490, 253, 252, 238); // White area under tags button yellow
                //PixelColorCheck p4 = new PixelColorCheck(830, 368, 101, 29, 29); // Heart icon brown maroon 
                PixelColorCheck p5 = new PixelColorCheck(1770, 600, 0, 153, 130); // area to the far right teal
                PixelColorCheck p1C = GenerateCompPixel(bmap, p1.X, p1.Y);
                PixelColorCheck p2C = GenerateCompPixel(bmap, p2.X, p2.Y);
                PixelColorCheck p3C = GenerateCompPixel(bmap, p3.X, p3.Y);
                //PixelColorCheck p4C = GenerateCompPixel(bmap, p4.X, p4.Y);
                PixelColorCheck p5C = GenerateCompPixel(bmap, p5.X, p5.Y);

                //Vec3b pixel = frame.At<Vec3b>(1290, 935);
                //byte[] pixel = new byte[3];
                //bmap.CopyPixels(new Int32Rect(1695, 580, 1, 1), pixel, 3, 0);


                //MessageBox.Show("1Comp Result: " + p1.CompareColor(p1C) + "\n" +
                //    "p1: " + p1.R + ", " + p1.G + ", " + p1.B + "\n" +
                //    "p2: " + p1C.R + ", " + p1C.G + ", " + p1C.B);

                double totalComp = (p1.CompareColor(p1C) + p2.CompareColor(p2C) + p3.CompareColor(p3C) /*+ p4.CompareColor(p4C)*/ + p5.CompareColor(p5C)) / 4;

                //MessageBox.Show("Pixel Colors - > 1: " + pixel[0] + "  2: " + pixel[1] + "  3: " + pixel[2]);

                //MessageBox.Show("Total LvlScreen Comp%: " + totalComp);
                //if (totalComp > perMatchAllowed)
                //    MessageBox.Show("LvlScreen Detected");
                return totalComp > perMatchAllowed;
                //PixelColorCheck p1C = new PixelColorCheck(1290, 935, frame.at)
            }
            //
            // If Lvl Screen detected, check for Report Button hover
            //
            bool checkPopLvlScreenRpt(BitmapSource bmap)
            {
                //
                // Coordinates for 4 corners of report box
                //
                PixelColorCheck p1 = GenerateCompPixel(bmap, 464, 969);
                PixelColorCheck p2 = GenerateCompPixel(bmap, 646, 969);
                PixelColorCheck p3 = GenerateCompPixel(bmap, 464, 1026);
                PixelColorCheck p4 = GenerateCompPixel(bmap, 646, 1026);

                //MessageBox.Show("Total Report Comp%: " + totalComp);

                // Check that they are nearly identical and mostly red
                //MessageBox.Show("Report Corner Match: " + (p1.CompareColor(p2) + p2.CompareColor(p3) + p3.CompareColor(p4)) / 3);
                if ((p1.CompareColor(p2) + p2.CompareColor(p3) + p3.CompareColor(p4)) / 3 < perMatchAllowed)
                    return false;

                //MessageBox.Show("Is Red? : " + (p1.R) + " " + (p1.G) + " " + (p1.B));
                //if (p1.R > 200 && p1.G < 150 && p1.B < 150)
                //    MessageBox.Show("LvlScreenRpt Detected");

                // Check for Red
                return ((p1.R > 200) && (p1.G < 150) && (p1.B < 150));
                //MessageBox.Show("Comp Result: \n" +
                //    "p1: " + p1.R + ", " + p1.G + ", " + p1.B);
            }

            lvlStartScreen = checkLvlStartScreen(bmap);
            if (lvlStartScreen)
            {
                return ScreenState.LevelStart;
            }

            lvlPlayedScreen = checkLvlPlayedScreen(bmap);
            lvlPlayedScreenRpt = false;
            if (lvlPlayedScreen)
            {
                lvlPlayedScreenRpt = checkLvlPlayedScreenRpt(bmap);
                return lvlPlayedScreenRpt ? ScreenState.LvlPlayedScreenRpt : ScreenState.LvlPlayedScreen;
            }

            lvlScreen = checkLvlScreen(bmap);
            lvlScreenRpt = false;
            if (lvlScreen)
            {
                lvlScreenRpt = checkLvlScreenRpt(bmap);
                //MessageBox.Show(lvlScreenRpt.ToString());

                return lvlScreenRpt ? ScreenState.LvlScreenRpt : ScreenState.LvlScreen;
            }

            lvlPopScreen = checkPopLvlScreen(bmap);
            lvlPopScreenRpt = false;
            if (lvlPopScreen)
            {
                lvlPopScreenRpt = checkPopLvlScreenRpt(bmap);
                return lvlPopScreenRpt ? ScreenState.LvlScreenPopRpt : ScreenState.LvlScreenPop;
            }
            return ScreenState.NoScreen;

        }
        static public PixelColorCheck GenerateCompPixel(BitmapSource bmap, int x, int y)
        {
            byte[] pixel = new byte[3];
            bmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 3, 0);
            PixelColorCheck pC = new PixelColorCheck(x, y, pixel[2], pixel[1], pixel[0]);
            return pC;
        }
        //
        // Break down Mat image based on the screen state, run sections thru OCR
        // then set all level values
        //
        static public void BreakDownImage(OpenCvSharp.Mat frame, ScreenState state, BitmapSource bmap)
        {
            string lvlCode, lvlName, lvlCreator = "";
            Level lvl;
            PixelColorCheck flagEnd;
            MainWindow mainWin = (MainWindow)Application.Current.MainWindow;

            void ReadLvlStartScn ()
            {
                lvlCode = SubImageText(frame, 110, 265, 263, 40, "code");
                lvlCode = lvlCode.Replace(" ", "");
                lvlCode = lvlCode.Replace("\n", "");
                if (!Regex.IsMatch(lvlCode, @"^[A-Z0-9]{3}-[A-Z0-9]{3}-[A-Z0-9]{3}$"))
                    return;
                lvlName = SubImageText(frame, 325, 140, 1366, 64);
                lvlCreator = SubImageText(frame, 1000, 265, 600, 50);
                //MessageBox.Show(lvlCode + '\n' + lvlName + '\n' + lvlCreator);
                lvl = new Level(lvlCode, lvlName, lvlCreator);
                lvl.Active = true;

                //mainWin.ActiveLevel = lvl;
                if (mainWin.ActiveLevel.Code == lvlCode)
                    return;

                lvl.LastPlayed = DateTime.Now;
                lvl.FirstPlayed = DateTime.Now;
                if (mainWin.LvlViewEndless)
                {
                    Utils.OpenLink(lvl.Link);
                    lvl.AutoOpened = true;
                }
                mainWin.UpdateActiveLevel(lvl);

                if (mainWin.LogAll)
                {
                    Utils.UpdateLog();
                }
            }
            void ReadLvlScn() // For Hot and New course screen
            {
                lvlCode = SubImageText(frame, 1088, 737, 250, 40, "code");
                lvlCode = lvlCode.Replace(" ", "");
                lvlCode = lvlCode.Replace("\n", "");
                if (!Regex.IsMatch(lvlCode, @"^[A-Z0-9]{3}-[A-Z0-9]{3}-[A-Z0-9]{3}$"))
                    return;
                if (mainWin.ActiveLevel.Code == lvlCode)
                    return;
                flagEnd = GetCreatorStart(bmap);
                lvlName = SubImageText(frame, 504, 275, 875, 50);
                lvlCreator = SubImageText(frame, flagEnd.X, 456, 1610 - flagEnd.X, 52);

                //MessageBox.Show(lvlCode + '\n' + lvlName + '\n' + lvlCreator);
                lvl = new Level(lvlCode, lvlName, lvlCreator);
                lvl.Active = true;

                //mainWin.ActiveLevel = lvl;
                mainWin.UpdateActiveLevel(lvl);
            }
            void ReadLvlPopScn() // For Hot and New course screen
            {

                //MessageBox.Show(SubImageText(frame, 1177, 737, 250, 40));
                lvlCode = SubImageText(frame, 1177, 737, 250, 40, "code");
                lvlCode = lvlCode.Replace(" ", "");
                lvlCode = lvlCode.Replace("\n", "");
                if (!Regex.IsMatch(lvlCode, @"^[A-Z0-9]{3}-[A-Z0-9]{3}-[A-Z0-9]{3}$"))
                    return;
                if (mainWin.ActiveLevel.Code == lvlCode)
                    return;
                flagEnd = GetCreatorStart(bmap);
                lvlName = SubImageText(frame, 593, 271, 1044, 57);
                lvlCreator = SubImageText(frame, flagEnd.X, 459, 1699 - flagEnd.X, 41); //1699
                //MessageBox.Show(lvlCode + '\n' + lvlName + '\n' + lvlCreator);
                lvl = new Level(lvlCode, lvlName, lvlCreator);
                lvl.Active = true;

                //mainWin.ActiveLevel = lvl;
                mainWin.UpdateActiveLevel(lvl);
            }

            switch (state)
            {
                case ScreenState.LevelStart:
                    //MessageBox.Show(SubImageText(frame, 110, 265, 263, 40));
                    ReadLvlStartScn();
                    break;
                case ScreenState.LvlScreen:
                    ReadLvlScn();
                    break;
                case ScreenState.LvlScreenRpt:
                    ReadLvlScn();
              
                    if (mainWin.LvlViewReport && !mainWin.ActiveLevel.AutoOpened)
                    {
                        Utils.OpenLink(mainWin.ActiveLevel.Link);
                        mainWin.ActiveLevel.AutoOpened = true;
                    }
                    break;
                case ScreenState.LvlScreenPop:
                    ReadLvlPopScn();
                    break;
                case ScreenState.LvlScreenPopRpt:
                    ReadLvlPopScn();

                    if (mainWin.LvlViewReport && !mainWin.ActiveLevel.AutoOpened)
                    {
                        Utils.OpenLink(mainWin.ActiveLevel.Link);
                        mainWin.ActiveLevel.AutoOpened = true;
                    }
                    break;
                default:
                    break;

            }
            //byte[] pixel = new byte[3];
            //bmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 3, 0);
            //PixelColorCheck pC = new PixelColorCheck(x, y, pixel[2], pixel[1], pixel[0]);
            //return pC;
        }
        static public string SubImageText(OpenCvSharp.Mat frame, int x, int y, int width, int height, string type = "text")
        {
            OpenCvSharp.Rect roi = new OpenCvSharp.Rect(x, y, width, height); // define the rectangle to crop (x, y, width, height)
            OpenCvSharp.Mat subMat = frame.SubMat(roi); // create a sub-matrix that represents the rectangle
            OpenCvSharp.Mat clonedMat = subMat.Clone();

            OpenCvSharp.Size newSize = new OpenCvSharp.Size(clonedMat.Width * 2, clonedMat.Height * 2);
            OpenCvSharp.Size newCodeSize = new OpenCvSharp.Size(clonedMat.Width * 4, clonedMat.Height * 4);

            Cv2.Resize(clonedMat, clonedMat, type == "code" ? newCodeSize : newSize, interpolation: InterpolationFlags.Linear);
            Cv2.CvtColor(clonedMat, clonedMat, ColorConversionCodes.BGR2GRAY);
            var binary = clonedMat.Threshold(0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            //Cv2.ImShow("Large View", binary);
            //using (var dst = new OpenCvSharp.Mat())
            //{
            //    // Define the kernel for sharpening
            //    var kernel = new OpenCvSharp.Mat(3, 3, MatType.CV_32F, new float[] {
            //        -1, -1, -1,
            //        -1,  9, -1,
            //        -1, -1, -1 });
            //    //Cv2.Filter2D(clonedMat, dst, -1, kernel);
            //    //Cv2.Filter2D(dst, dst, -1, kernel);
            //    //Cv2.Filter2D(dst, dst, -1, kernel);

            //if (type == "code")
            //    Cv2.ImShow("Large View", binary);
            var codeTxt = GetOCRText(binary, type);
            codeTxt = codeTxt.Replace("\n", "");
            return codeTxt;
            //}
            // Apply adaptive thresholding to create a binary image
            //OpenCvSharp.Mat binaryImage = new OpenCvSharp.Mat();
            //Cv2.AdaptiveThreshold(clonedMat, binaryImage, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 2);

            //// Increase the contrast using the CLAHE algorithm
            //OpenCvSharp.Mat contrastImage = new OpenCvSharp.Mat();
            ////Cv2.CLAHE(binaryImage, 2.0, new OpenCvSharp.Size(width, height), contrastImage);
            //var clahe = Cv2.CreateCLAHE(clipLimit: 1.0, tileGridSize: new OpenCvSharp.Size(width, height));

            //// Apply the CLAHE algorithm to the grayscale image to increase contrast
            ////OpenCvSharp.Mat contrastImage = new OpenCvSharp.Mat();
            //clahe.Apply(binaryImage, contrastImage);


        }

        //
        // There's is not a specific rect box to capture a creator's name without sometimes
        // capturing the country flag.  So detect where the flag and name's respective pixels start
        // Start at 1200, 461 and move leftwards.
        //
        static public PixelColorCheck GetCreatorStart(BitmapSource bmap)
        {
            int x = 1200;
            byte[] pixel = new byte[3];
            byte[] pixel2 = new byte[3];

            bmap.CopyPixels(new Int32Rect(1200, 461, 1, 1), pixel, 3, 0);
            PixelColorCheck startP = new PixelColorCheck(x, 461, pixel[2], pixel[1], pixel[0]);

            bmap.CopyPixels(new Int32Rect(x++, 461, 1, 1), pixel2, 3, 0);
            PixelColorCheck nextP = new PixelColorCheck(x, 461, pixel2[2], pixel2[1], pixel2[0]);

            while (startP.CompareColor(nextP) > 99)
            {
                startP = nextP;

                bmap.CopyPixels(new Int32Rect(x++, 461, 1, 1), pixel2, 3, 0);
                nextP = new PixelColorCheck(x, 461, pixel2[2], pixel2[1], pixel2[0]);
            }
            //MessageBox.Show("Flag Start: " + nextP.X + ", " + nextP.Y);

            //
            // Add the width of the flag plus some to the X value
            //
            nextP.X += 65;

            return nextP;

        }
    }








    // Set the language to be used for OCR
    //var engine = new TesseractEngine(@"path/to/tessdata", "eng", EngineMode.Default);

    //engine.SetVariable("tessedit_char_whitelist", "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
    //// Set the whitelist of characters to recognize

    //engine.Process(bmap)
    //ocrEngine.Init(@"path/to/tessdata", "eng", OcrEngineMode.Default);

    //// Recognize text from the bitmap using Tesseract OCR
    //ocrEngine.Recognize(bmap);

    //// Get the recognized text
    //string recognizedText = ocrEngine.GetText();
    //return recognizedText;
};
