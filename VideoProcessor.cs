﻿using System;
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
using Newtonsoft.Json;
using System.Net.Http;
using System.Data;
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

                var state = CheckScreenState(frame, bitmapSource);
                //MessageBox.Show(state.ToString());


                //
                // If screenState is anything other than NoScreen then start the OCR process
                // Break down the image first to speed up OCR
                //
                if (state != ScreenState.NoScreen)
                {
                    BreakDownImage(frame, state, bitmapSource);
                }
                mainWin.ScreenState = state;
                if (mainWin.ScreenState != mainWin.LastScreenState)
                {
                    if (mainWin.ScreenState == ScreenState.DeathMarker)
                    {
                        mainWin.ActiveLevel.DeathCnt++;
                        mainWin.Deaths.Content = mainWin.ActiveLevel.DeathCnt;
                        Utils.UpdateLog();
                    }
                    Utils.Log(mainWin.ScreenState.ToString(), true);
                    mainWin.LastScreenState = mainWin.ScreenState;
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
                //
                // For the MM2 code, use specially trained tessdata for conversion
                //
                using (var engine = new Engine(@"C:\Program Files\Tesseract-OCR\tessdata\train", Language.English, TesseractOCR.Enums.EngineMode.Default))
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
                using (var engine = new Engine(@"C:\Program Files\Tesseract-OCR\tessdata", "eng+jpn", TesseractOCR.Enums.EngineMode.Default))
                {
                    if (type == "num")
                        engine.SetVariable("tessedit_char_whitelist", "0123456789:.");
                    //else
                    //    Cv2.ImShow("Test", frame);
                    using (img)
                    //using (var img = Tesseract.Pix.LoadFromMemory(bmap))
                    {
                        using (var page = engine.Process(img))
                        {
                            ocrtext = page.Text;
                            //MessageBox.Show(ocrtext);
                        }
                    }
                }
            }


            return ocrtext;
        }
        //static public void CheckScreenState(OpenCvSharp.Mat frame)
        static public ScreenState CheckScreenState(OpenCvSharp.Mat frame, BitmapSource bmap)
        {
            var perMatchAllowed = 97; //97

            var lvlStartScreen = false;
            var lvlPlayedScreen = false;
            var lvlPlayedScreenRpt = false;
            var lvlScreen = false;
            var lvlScreenRpt = false;
            var lvlPopScreen = false;
            var lvlPopScreenRpt = false;

            var pause = false;
            var pauseBoo = false;
            var pauseHeart = false;
            var endScreen = false;
            var endScreenBoo = false;
            var endScreenHeart = false;
            var endScreenAlt = false;

            var deathMarker = false;


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
                PixelColorCheck p6 = new PixelColorCheck(797, 148, 129, 239, 227); // Upper Tab Teal Color

                PixelColorCheck p1C = GenerateCompPixel(bmap, p1.X, p1.Y);
                PixelColorCheck p2C = GenerateCompPixel(bmap, p2.X, p2.Y);
                PixelColorCheck p3C = GenerateCompPixel(bmap, p3.X, p3.Y);
                PixelColorCheck p4C = GenerateCompPixel(bmap, p4.X, p4.Y);
                PixelColorCheck p5C = GenerateCompPixel(bmap, p5.X, p5.Y);
                PixelColorCheck p6C = GenerateCompPixel(bmap, p6.X, p6.Y);

                //Vec3b pixel = frame.At<Vec3b>(1290, 935);
                //byte[] pixel = new byte[3];
                //bmap.CopyPixels(new Int32Rect(1695, 580, 1, 1), pixel, 3, 0);


                //MessageBox.Show("LvlScreenComp Result: " + p6.CompareColor(p1C) + "\n" +
                //    "p1: " + p6.R + ", " + p6.G + ", " + p6.B + "\n" +
                //    "p2: " + p6C.R + ", " + p6C.G + ", " + p6C.B);

                double totalComp = (p1.CompareColor(p1C) + p2.CompareColor(p2C) + p3.CompareColor(p3C) + p4.CompareColor(p4C) + p5.CompareColor(p5C) + p6.CompareColor(p6C)) / 6;

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
                PixelColorCheck p4 = new PixelColorCheck(797, 148, 0, 153, 131); // Upper Tab Teal Color
                PixelColorCheck p5 = new PixelColorCheck(1770, 600, 0, 153, 130); // area to the far right teal
                PixelColorCheck p1C = GenerateCompPixel(bmap, p1.X, p1.Y);
                PixelColorCheck p2C = GenerateCompPixel(bmap, p2.X, p2.Y);
                PixelColorCheck p3C = GenerateCompPixel(bmap, p3.X, p3.Y);
                PixelColorCheck p4C = GenerateCompPixel(bmap, p4.X, p4.Y);
                PixelColorCheck p5C = GenerateCompPixel(bmap, p5.X, p5.Y);

                //Vec3b pixel = frame.At<Vec3b>(1290, 935);
                //byte[] pixel = new byte[3];
                //bmap.CopyPixels(new Int32Rect(1695, 580, 1, 1), pixel, 3, 0);


                //MessageBox.Show("LvlScnPopComp Result: " + p5.CompareColor(p5C) + "\n" +
                //    "p1: " + p5.R + ", " + p5.G + ", " + p5.B + "\n" +
                //    "p2: " + p5C.R + ", " + p5C.G + ", " + p5C.B);

                double totalComp = (p1.CompareColor(p1C) + p2.CompareColor(p2C) + p3.CompareColor(p3C) + p4.CompareColor(p4C) + p5.CompareColor(p5C)) / 5;

                //MessageBox.Show("Pixel Colors - > 1: " + pixel[0] + "  2: " + pixel[1] + "  3: " + pixel[2]);

                //MessageBox.Show("Total LvlScreenPop Comp%: " + totalComp);
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
            bool checkPauseScreen(BitmapSource bmap) // check for pause screen
            {
                //
                // Coordinates and colors that represent the Browsing section Level View Screen
                //
                PixelColorCheck p1 = new PixelColorCheck(1843, 41, 13, 0, 0); // Top right close btn black
                PixelColorCheck p2 = new PixelColorCheck(1843, 66, 255, 255, 255); // Top right close btn white
                PixelColorCheck p3 = new PixelColorCheck(1799, 990, 255, 255, 255); // White Settings Icon
                PixelColorCheck p4 = new PixelColorCheck(1311, 827, 255, 204, 30); // Exit course btn yellow
                PixelColorCheck p1C = GenerateCompPixel(bmap, p1.X, p1.Y);
                PixelColorCheck p2C = GenerateCompPixel(bmap, p2.X, p2.Y);
                PixelColorCheck p3C = GenerateCompPixel(bmap, p3.X, p3.Y);
                PixelColorCheck p4C = GenerateCompPixel(bmap, p4.X, p4.Y);

                MessageBox.Show("1Comp Result: " + p3.CompareColor(p3C) + "\n" +
                    "p1: " + p3.R + ", " + p3.G + ", " + p3.B + "\n" +
                    "p2: " + p3C.R + ", " + p3C.G + ", " + p3C.B);

                double totalComp = (p1.CompareColor(p1C) + p2.CompareColor(p2C) + p3.CompareColor(p3C) + p4.CompareColor(p4C)) / 4;

                //MessageBox.Show("Pixel Colors - > 1: " + pixel[0] + "  2: " + pixel[1] + "  3: " + pixel[2]);

                //MessageBox.Show("Total PauseScreen Comp%: " + totalComp);
                //if (totalComp > perMatchAllowed)
                //    MessageBox.Show("LvlScreen Detected");
                return totalComp > perMatchAllowed;
                //PixelColorCheck p1C = new PixelColorCheck(1290, 935, frame.at)
            }
            bool checkPauseScreenBoo(BitmapSource bmap) // check if user has heart selected on pause screen
            {
                //
                // Coordinates and colors that represent the Browsing section Level View Screen
                //
                PixelColorCheck p1 = new PixelColorCheck(1364, 253, 95, 85, 177); // Left blue heart
                PixelColorCheck p2 = new PixelColorCheck(1724, 276, 85, 85, 87); // Right gray heart
                PixelColorCheck p1C = GenerateCompPixel(bmap, p1.X, p1.Y);
                PixelColorCheck p2C = GenerateCompPixel(bmap, p2.X, p2.Y);

                //MessageBox.Show("1Comp Result: " + p2.CompareColor(p2C) + "\n" +
                //    "p1: " + p2.R + ", " + p2.G + ", " + p2.B + "\n" +
                //    "p2: " + p2C.R + ", " + p2C.G + ", " + p2C.B);

                double totalComp = (p1.CompareColor(p1C) + p2.CompareColor(p2C)) / 2;

                //MessageBox.Show("Pixel Colors - > 1: " + pixel[0] + "  2: " + pixel[1] + "  3: " + pixel[2]);

                //MessageBox.Show("Total PauseScreen Comp%: " + totalComp);
                //if (totalComp > perMatchAllowed)
                //    MessageBox.Show("LvlScreen Detected");
                return totalComp > perMatchAllowed;
                //PixelColorCheck p1C = new PixelColorCheck(1290, 935, frame.at)
            }
            bool checkPauseScreenHeart(BitmapSource bmap) // check if user has heart selected on pause screen
            {
                //
                // Coordinates and colors that represent the Browsing section Level View Screen
                //
                PixelColorCheck p1 = new PixelColorCheck(1702, 262, 255, 98, 98); // Right red heart
                PixelColorCheck p2 = new PixelColorCheck(1414, 262, 85, 85, 87); // Left gray boo
                PixelColorCheck p1C = GenerateCompPixel(bmap, p1.X, p1.Y);
                PixelColorCheck p2C = GenerateCompPixel(bmap, p2.X, p2.Y);

                //MessageBox.Show("1Comp Result: " + p2.CompareColor(p2C) + "\n" +
                //    "p1: " + p2.R + ", " + p2.G + ", " + p2.B + "\n" +
                //    "p2: " + p2C.R + ", " + p2C.G + ", " + p2C.B);

                double totalComp = (p1.CompareColor(p1C) + p2.CompareColor(p2C)) / 2;

                //MessageBox.Show("Pixel Colors - > 1: " + pixel[0] + "  2: " + pixel[1] + "  3: " + pixel[2]);

                //MessageBox.Show("Total PauseScreen Comp%: " + totalComp);
                //if (totalComp > perMatchAllowed)
                //    MessageBox.Show("LvlScreen Detected");
                return totalComp > perMatchAllowed;
                //PixelColorCheck p1C = new PixelColorCheck(1290, 935, frame.at)
            }
            bool checkEndScreen(BitmapSource bmap) // check for pause screen
            {
                //
                // Coordinates and colors that represent the Browsing section Level View Screen
                //
                PixelColorCheck p1 = new PixelColorCheck(22, 237, 255, 206, 29); // 
                PixelColorCheck p2 = new PixelColorCheck(1887, 245, 255, 206, 29); // 
                PixelColorCheck p3 = new PixelColorCheck(22, 354, 255, 206, 29); // 
                PixelColorCheck p1C = GenerateCompPixel(bmap, p1.X, p1.Y);
                PixelColorCheck p2C = GenerateCompPixel(bmap, p2.X, p2.Y);
                PixelColorCheck p3C = GenerateCompPixel(bmap, p3.X, p3.Y);

                //MessageBox.Show("1Comp Result: " + p3.CompareColor(p3C) + "\n" +
                //    "p1: " + p3.R + ", " + p3.G + ", " + p3.B + "\n" +
                //    "p2: " + p3C.R + ", " + p3C.G + ", " + p3C.B);

                double totalCompEnd = (p1.CompareColor(p1C) + p2.CompareColor(p2C) + p3.CompareColor(p3C)) / 3;
                if (totalCompEnd > perMatchAllowed)
                {
                    // On end screen, check if on alternate end screen (comments on bottom)
                    // check the blue at the top
                    PixelColorCheck p4 = new PixelColorCheck(960, 20, 255, 206, 29); // 
                    PixelColorCheck p4C = GenerateCompPixel(bmap, p3.X, p3.Y);
                    if (p4.CompareColor(p4C) < perMatchAllowed)
                    {
                        endScreenAlt = true;
                    }
                    return true;

                }
                return false;
                //MessageBox.Show("Pixel Colors - > 1: " + pixel[0] + "  2: " + pixel[1] + "  3: " + pixel[2]);

                //MessageBox.Show("Total EndScreen Comp%: " + totalComp);
                //if (totalComp > perMatchAllowed)
                //    MessageBox.Show("LvlScreen Detected");
                //return totalComp > perMatchAllowed;
                //PixelColorCheck p1C = new PixelColorCheck(1290, 935, frame.at)
            }
            bool checkEndScreenHeart(BitmapSource bmap) // check if user has heart selected on pause screen
            {
                //
                // Coordinates and colors that represent the Browsing section Level View Screen
                //
                // sub 193 from y value if alt screen
                PixelColorCheck p1 = new PixelColorCheck(459, endScreenAlt ? 299 : 492, 255, 99, 99); // Left red heart
                PixelColorCheck p2 = new PixelColorCheck(215, endScreenAlt ? 294 : 487, 199, 164, 106); // Booed greyed out

                PixelColorCheck p1C = GenerateCompPixel(bmap, p1.X, p1.Y);
                PixelColorCheck p2C = GenerateCompPixel(bmap, p2.X, p2.Y);

                //MessageBox.Show("1Comp Result: " + p1.CompareColor(p1C) + "\n" +
                //    "p1: " + p1.R + ", " + p1.G + ", " + p1.B + "\n" +
                //    "p2: " + p1C.R + ", " + p1C.G + ", " + p1C.B);

                double totalComp = (p1.CompareColor(p1C) + p2.CompareColor(p2C)) / 2;

                //MessageBox.Show("Pixel Colors - > 1: " + pixel[0] + "  2: " + pixel[1] + "  3: " + pixel[2]);

                //MessageBox.Show("Total EndScreenHeart Comp%: " + totalComp);
                //if (totalComp > perMatchAllowed)
                //    MessageBox.Show("LvlScreen Detected");
                return totalComp > perMatchAllowed;
            }
            bool checkEndScreenBoo(BitmapSource bmap) // check if user has heart selected on pause screen
            {
                //
                // Coordinates and colors that represent the Browsing section Level View Screen
                //
                PixelColorCheck p1 = new PixelColorCheck(159, endScreenAlt ? 292 : 485, 95, 85, 177); // Left blue heart
                PixelColorCheck p2 = new PixelColorCheck(490, endScreenAlt ? 320 : 513, 199, 164, 106); // Hearted greyed out
                PixelColorCheck p1C = GenerateCompPixel(bmap, p1.X, p1.Y);
                PixelColorCheck p2C = GenerateCompPixel(bmap, p2.X, p2.Y);

                //MessageBox.Show("1Comp Result: " + p2.CompareColor(p2C) + "\n" +
                //    "p1: " + p2.R + ", " + p2.G + ", " + p2.B + "\n" +
                //    "p2: " + p2C.R + ", " + p2C.G + ", " + p2C.B);

                double totalComp = (p1.CompareColor(p1C) + p2.CompareColor(p2C)) / 2;

                //MessageBox.Show("Pixel Colors - > 1: " + pixel[0] + "  2: " + pixel[1] + "  3: " + pixel[2]);

                //MessageBox.Show("Total EndScreenBoo Comp%: " + totalComp);
                //if (totalComp > perMatchAllowed)
                //    MessageBox.Show("LvlScreen Detected");
                return totalComp > perMatchAllowed;
            }
            bool checkDeathMarker() // check if user has heart selected on pause screen
            {
                //
                // Check for the death indicator on screen
                //
                try
                {
                    using (var exampleImg = new OpenCvSharp.Mat("MarioDeadIndicator.png", ImreadModes.Grayscale))
                    using (frame)
                    {
                        OpenCvSharp.Mat thresholdedExampleImg = new OpenCvSharp.Mat();
                        Cv2.CvtColor(exampleImg, exampleImg, ColorConversionCodes.BGRA2BGR);
                        Cv2.Threshold(exampleImg, thresholdedExampleImg, 128, 255, ThresholdTypes.Binary);

                        if (frame.Width < thresholdedExampleImg.Width || frame.Height < thresholdedExampleImg.Height)
                        {
                            MessageBox.Show("Error: Example image is larger than the search image.");
                            return false;
                        }

                        if (frame.Type() != thresholdedExampleImg.Type())
                        {
                            MessageBox.Show("Error: Images are of different types.");
                            return false;
                        }

                        OpenCvSharp.Mat result = new OpenCvSharp.Mat();
                        Cv2.MatchTemplate(frame, thresholdedExampleImg, result, TemplateMatchModes.CCoeffNormed);

                        double minVal, maxVal;
                        OpenCvSharp.Point minLoc, maxLoc;
                        Cv2.MinMaxLoc(result, out minVal, out maxVal, out minLoc, out maxLoc);
                        //Utils.Log(maxVal.ToString(), true);
                        if (maxVal > 0.6) // adjust the threshold as needed
                        {
                            OpenCvSharp.Rect detectionRect = new OpenCvSharp.Rect(maxLoc.X, maxLoc.Y, thresholdedExampleImg.Cols, thresholdedExampleImg.Rows);
                            Cv2.Rectangle(frame, detectionRect, new Scalar(0, 255, 0), 2);
                            //Cv2.ImShow("Large View", frame);

                            OpenCvSharp.Point center = new OpenCvSharp.Point(detectionRect.X + detectionRect.Width / 2, detectionRect.Y + detectionRect.Height / 2);
                            OpenCvSharp.Size regionSize = new OpenCvSharp.Size(10, 10); // adjust as needed
                            OpenCvSharp.Mat region = new OpenCvSharp.Mat();
                            Cv2.GetRectSubPix(frame, regionSize, center, region);

                            // Check color of pixel at center of region
                            Vec3b pixelColor = region.At<Vec3b>(5, 5); // center pixel of 10x10 region
                            //MessageBox.Show(pixelColor.Item0 + " " + pixelColor.Item1 + " " + pixelColor.Item2);
                            Utils.Log("Detected Death Marker color: " + pixelColor.Item0 + ", " + pixelColor.Item1 + ", " + pixelColor.Item2);
                            // 9 5 255
                            if (pixelColor.Item0 < 15 && pixelColor.Item1 < 15 && pixelColor.Item2 > 250) // check for red pixel
                            {
                                return true;
                            }
                            //Cv2.ImShow("Ex View", result);
                            //largerImg.SaveImage("result.jpg");
                        }
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return false;
                }

            }

            lvlStartScreen = checkLvlStartScreen(bmap);
            if (lvlStartScreen)
            {
                return ScreenState.LevelStart;
            }

            lvlPlayedScreen = checkLvlPlayedScreen(bmap);
            if (lvlPlayedScreen)
            {
                lvlPlayedScreenRpt = checkLvlPlayedScreenRpt(bmap);
                return lvlPlayedScreenRpt ? ScreenState.LvlPlayedScreenRpt : ScreenState.LvlPlayedScreen;
            }

            lvlScreen = checkLvlScreen(bmap);
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
            pause = checkPauseScreen(bmap);
            if (pause)
            {
                pauseBoo = checkPauseScreenBoo(bmap);
                if (!pauseBoo)
                    pauseHeart = checkPauseScreenHeart(bmap);
                //MessageBox.Show("Pause Detected");
                // check for heart or boo state
                //lvlPopScreenRpt = checkPopLvlScreenRpt(bmap);
                //return lvlPopScreenRpt ? ScreenState.LvlScreenPopRpt : ScreenState.LvlScreenPop;
                if (pauseBoo)
                    return ScreenState.PauseBoo;
                else if (pauseHeart)
                    return ScreenState.PauseHeart;

                return ScreenState.Pause;
            }
            endScreen = checkEndScreen(bmap);
            if (endScreen)
            {
                endScreenBoo = checkEndScreenBoo(bmap);
                if (!endScreenBoo)
                    endScreenHeart = checkEndScreenHeart(bmap);
                //MessageBox.Show("Pause Detected");
                // check for heart or boo state
                //lvlPopScreenRpt = checkPopLvlScreenRpt(bmap);
                //return lvlPopScreenRpt ? ScreenState.LvlScreenPopRpt : ScreenState.LvlScreenPop;
                if (endScreenBoo)
                    return endScreenAlt ? ScreenState.EndScreenBooAlt : ScreenState.EndScreenBoo;
                else if (endScreenHeart)
                    return endScreenAlt ? ScreenState.EndScreenHeartAlt : ScreenState.EndScreenHeart;

                return endScreenAlt ? ScreenState.EndScreenAlt : ScreenState.EndScreen;
            }
            if (checkDeathMarker())
            {
                Utils.Log("Logged +1 Death", true);
                return ScreenState.DeathMarker;
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
                lvlCode = SubImageText(frame, 110, 265, 310, 40, "code");
                lvlCode = lvlCode.Replace(" ", "");
                lvlCode = lvlCode.Replace("\n", "");
                if (!Regex.IsMatch(lvlCode, @"^[A-Z0-9]{3}-[A-Z0-9]{3}-[A-Z0-9]{3}$"))
                    return;
                var alreadyActive = mainWin.ActiveLevel.Code == lvlCode;
                if (alreadyActive) // user started level from a previous level scn
                {
                    mainWin.ActiveLevel.LastPlayed = DateTime.Now;
                    //mainWin.ActiveLevel.FirstPlayed = DateTime.Now;
                }
                else // create new level object
                {
                    lvlName = SubImageText(frame, 325, 140, 1366, 64);
                    lvlCreator = SubImageText(frame, 1000, 265, 600, 50);
                    //MessageBox.Show(lvlCode + '\n' + lvlName + '\n' + lvlCreator);
                    lvl = new Level(lvlCode, lvlName, lvlCreator);
                    lvl.Active = true;


                    lvl.LastPlayed = DateTime.Now;
                    lvl.FirstPlayed = DateTime.Now;
                    mainWin.UpdateActiveLevel(lvl);
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
                //
                // Found unique issue where some users do not have flags..
                //
                if (flagEnd.X > 1600)
                {
                    flagEnd.X = 1161;
                    flagEnd.Y = 456;
                }
                lvlName = SubImageText(frame, 504, 275, 875, 50);
                lvlCreator = SubImageText(frame, flagEnd.X, 456, 1610 - flagEnd.X, 52);

                //MessageBox.Show(lvlCode + '\n' + lvlName + '\n' + lvlCreator);
                lvl = new Level(lvlCode, lvlName, lvlCreator);
                lvl.Active = true;

                //mainWin.ActiveLevel = lvl;
                mainWin.UpdateActiveLevel(lvl);
            }
            void ReadLvlPopScn() // For Popular course screen
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
                //
                // Found unique issue where some users do not have flags..
                //
                if (flagEnd.X > 1600)
                {
                    flagEnd.X = 1161;
                    flagEnd.Y = 456;
                }
                lvlName = SubImageText(frame, 593, 271, 1044, 57);
                lvlCreator = SubImageText(frame, flagEnd.X, 459, 1699 - flagEnd.X, 41); //1699
                //MessageBox.Show(lvlCode + '\n' + lvlName + '\n' + lvlCreator);
                lvl = new Level(lvlCode, lvlName, lvlCreator);
                lvl.Active = true;

                //mainWin.ActiveLevel = lvl;
                mainWin.UpdateActiveLevel(lvl);
            }
            void ReadLvlPlayedScn() // For Popular course screen
            {

                //MessageBox.Show(SubImageText(frame, 1177, 737, 250, 40));
                lvlCode = SubImageText(frame, 1064, 705, 250, 40, "code");
                lvlCode = lvlCode.Replace(" ", "");
                lvlCode = lvlCode.Replace("\n", "");

                if (!Regex.IsMatch(lvlCode, @"^[A-Z0-9]{3}-[A-Z0-9]{3}-[A-Z0-9]{3}$"))
                    return;
                if (mainWin.ActiveLevel.Code == lvlCode)
                    return;
                flagEnd = GetCreatorStart(bmap);
                //
                // Found unique issue where some users do not have flags..
                //
                if (flagEnd.X > 1600)
                {
                    flagEnd.X = 1161;
                    flagEnd.Y = 456;
                }
                lvlName = SubImageText(frame, 477, 235, 1044, 57);
                lvlCreator = SubImageText(frame, flagEnd.X, 428, 1586 - flagEnd.X, 41); //1586 end of possible txt area
                //MessageBox.Show(lvlCode + '\n' + lvlName + '\n' + lvlCreator);
                lvl = new Level(lvlCode, lvlName, lvlCreator);
                lvl.Active = true;

                //mainWin.ActiveLevel = lvl;
                mainWin.UpdateActiveLevel(lvl);
            }
            void ReadEndScn() // For Popular course screen
            {

                //MessageBox.Show(SubImageText(frame, 1177, 737, 250, 40));
                var worldRec = SubImageText(frame, 917, state == ScreenState.EndScreenAlt ? 337 : 530, 309, 62, "num");
                worldRec = worldRec.Replace(" ", "");
                worldRec = worldRec.Replace("\n", "");
                //
                // End screen will be detected before user record time is shown
                // make sure it has numbers and can be read.
                //
                if (!System.Text.RegularExpressions.Regex.IsMatch(worldRec, @"[\d.:]+"))
                    return;
                //MessageBox.Show(worldRec);

                string[] timeParts = worldRec.Split(':');
                int minutes = int.Parse(timeParts[0]);
                string[] secondsParts = timeParts[1].Split('.');
                int seconds = int.Parse(secondsParts[0]);
                int milliseconds = int.Parse(secondsParts[1]);

                long ticks = (long)((minutes * 60 + seconds) * TimeSpan.TicksPerSecond + milliseconds * TimeSpan.TicksPerMillisecond);
                TimeSpan recordTime = new TimeSpan(ticks);

                if (mainWin.ActiveLevel.RecordTime != recordTime)
                {
                    mainWin.ActiveLevel.RecordTime = recordTime;

                    if (mainWin.LogAll)
                    {
                        Utils.UpdateLog();
                        Utils.Log("Updated Data File WorldRecord: " + worldRec, true);
                    }
                }
                //MessageBox.Show(ticks.ToString());


                //mainWin.ActiveLevel = lvl;
                //mainWin.UpdateActiveLevel(lvl);
            }

            switch (state)
            {
                case ScreenState.LevelStart:
                    //MessageBox.Show(SubImageText(frame, 110, 265, 263, 40));
                    ReadLvlStartScn();
                    if (mainWin.LvlViewEndless && !mainWin.ActiveLevel.AutoOpened)
                    {
                        Utils.OpenLink(mainWin.ActiveLevel.Link);
                        mainWin.ActiveLevel.AutoOpened = true;
                    }
                    if (!mainWin.ActiveLevel.Logged && mainWin.LogAll)
                    {
                        Utils.UpdateLog();
                        mainWin.ActiveLevel.Logged = true;
                    }
                    Utils.CheckExistingLog();
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
                case ScreenState.LvlPlayedScreen:
                    ReadLvlPlayedScn();
                    break;
                case ScreenState.LvlPlayedScreenRpt:
                    ReadLvlPlayedScn();

                    if (mainWin.LvlViewReport && !mainWin.ActiveLevel.AutoOpened)
                    {
                        Utils.OpenLink(mainWin.ActiveLevel.Link);
                        mainWin.ActiveLevel.AutoOpened = true;
                    }
                    break;
                case ScreenState.PauseBoo:
                    if (mainWin.LogAll && mainWin.ActiveLevel.Hearted == null)
                    {
                        mainWin.ActiveLevel.Hearted = "B";
                        Utils.UpdateLog();
                    }
                    break;
                case ScreenState.PauseHeart:
                    //MessageBox.Show("mainWin.LogAll && mainWin.ActiveLevel.Hearted == null");
                    if (mainWin.LogAll && mainWin.ActiveLevel.Hearted == null)
                    {
                        //MessageBox.Show("mainWin.ActiveLevel.Hearted");
                        mainWin.ActiveLevel.Hearted = "H";
                        Utils.UpdateLog();
                    }
                    break;
                case ScreenState.EndScreen:
                    ReadEndScn();
                    break;
                case ScreenState.EndScreenBoo:
                    ReadEndScn();
                    if (mainWin.LogAll && mainWin.ActiveLevel.Hearted == null)
                    {
                        mainWin.ActiveLevel.Hearted = "B";
                        Utils.UpdateLog();
                    }
                    break;
                case ScreenState.EndScreenHeart:
                    ReadEndScn();
                    //MessageBox.Show("mainWin.LogAll && mainWin.ActiveLevel.Hearted == null");
                    if (mainWin.LogAll && mainWin.ActiveLevel.Hearted == null)
                    {
                        //MessageBox.Show("mainWin.ActiveLevel.Hearted");
                        mainWin.ActiveLevel.Hearted = "H";
                        Utils.UpdateLog();
                    }
                    break;
                default:
                    break;

            }
            if (!mainWin.ActiveLevel.PulledInfo && state != ScreenState.NoScreen)
            {
                Utils.GrabMM2Info();
                mainWin.ActiveLevel.SMM2InfoSuccess = 2;
                mainWin.ActiveLevel.PulledInfo = true;
                Utils.Log("Name: " + mainWin.ActiveLevel.Name);
                Utils.Log("Code: " + mainWin.ActiveLevel.Code);
                Utils.Log("Creator: " + mainWin.ActiveLevel.Creator);
            }
            if (!mainWin.ActiveLevel.TransTaskSent && state != ScreenState.NoScreen)
            {
                if (Utils.ContainsJapanChar(mainWin.ActiveLevel.Name))
                {
                    Utils.GrabTranslation();
                    mainWin.ActiveLevel.GoogleTransSuccess = 2;
                    mainWin.ActiveLevel.TransTaskSent = true;
                }
            }
            if (mainWin.ActiveLevel.InfoTask != null && mainWin.ActiveLevel.InfoTask.IsCompleted && mainWin.ActiveLevel.InfoTask.IsCanceled == false)
            {
                //Set new info accordingly
                //MessageBox.Show(mainWin.ActiveLevel.SMM2InfoSuccess.ToString());
                if (mainWin.ActiveLevel.SMM2InfoSuccess == 2)
                    Utils.HandleResponse();
            }
            if (mainWin.ActiveLevel.TransTask != null && mainWin.ActiveLevel.TransTask.IsCompleted && mainWin.ActiveLevel.TransTask.IsCanceled == false)
            {
                //Set new info accordingly
                if (mainWin.ActiveLevel.GoogleTransSuccess == 2)
                    Utils.HandleTransResponse();
            }
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

            //if (type != "code")
            //    Cv2.ImShow("Large View", binary);
            var codeTxt = GetOCRText(binary, type);
            //MessageBox.Show(codeTxt);
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
        // Start at 1200, 461 and move leftwards. 430 for Played screen
        //
        static public PixelColorCheck GetCreatorStart(BitmapSource bmap)
        {
            MainWindow mainWin = (MainWindow)Application.Current.MainWindow;

            int x = 1200;
            int y = 461;
            if (mainWin.ScreenState == ScreenState.LvlPlayedScreen || mainWin.ScreenState == ScreenState.LvlPlayedScreenRpt)
                y = 430;
            byte[] pixel = new byte[3];
            byte[] pixel2 = new byte[3];

            bmap.CopyPixels(new Int32Rect(1200, y, 1, 1), pixel, 3, 0);
            PixelColorCheck startP = new PixelColorCheck(x, 461, pixel[2], pixel[1], pixel[0]);

            bmap.CopyPixels(new Int32Rect(x++, y, 1, 1), pixel2, 3, 0);
            PixelColorCheck nextP = new PixelColorCheck(x, 461, pixel2[2], pixel2[1], pixel2[0]);

            while (startP.CompareColor(nextP) > 99)
            {
                startP = nextP;

                bmap.CopyPixels(new Int32Rect(x++, y, 1, 1), pixel2, 3, 0);
                nextP = new PixelColorCheck(x, y, pixel2[2], pixel2[1], pixel2[0]);
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
