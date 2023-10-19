using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using DirectShowLib;
using System.Drawing;
using System.Drawing.Imaging;
using OpenCvSharp.Internal;
using OfficeOpenXml.Drawing;
//using Emgu.CV;
//using OpenCvSharp.Extensions;

namespace MM2Buddy
{
    /// <summary>
    /// Interaction logic for ScreenOverlayWin.xaml
    /// </summary>
    /// 
    public partial class ScreenOverlayWin : System.Windows.Window
    {
        private VideoCapture capture;
        //private VideoWriter videoWriter;

        private const int WindowWidth = 960;
        private const int WindowHeight = 540;



        //private readonly Scalar _ballColor = new Scalar(0, 0, 255); // Red color
        //private readonly Random _random = new Random();
        //private Mat _mat = new Mat(WindowHeight, WindowWidth, MatType.CV_8UC4);
        //private string textToDisplay = "Hello, OpenCvSharp!";
        //private Scalar textColor = new Scalar(255, 255, 255, 255);
        //private double textAlpha = 0;
        //private double fadeDuration = 90000; // 2 seconds
        //private DateTime startTime;
        //private bool isCodeFadingIn = false;
        //private bool isCodeFadingOut = false;
        private Mat greenMat;
        private TextBlock textBlock;
        private DispatcherTimer fadeTimer;
        private double currentOpacity = 0.0;
        private Grid grid;

        public ScreenOverlayWin()
        {
            int frameRate = 5;
            int width = 1920;
            int height = 1080;

            InitializeComponent();
            Loaded += MainWindow_Loaded;
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            //// Create a blank window
            //_mat.SetTo(new Scalar(64, 177, 0, 255)); // green screen color

            //// Initialize the VideoCapture object
            //capture = new VideoCapture();
            //capture.Open(2); // Use 0 for the default camera, adjust accordingly if you have multiple cameras

            //if (capture.IsOpened())
            //{
            //    //capture.Set(VideoCaptureProperties.FrameWidth, 800);
            //    //capture.Set(VideoCaptureProperties.FrameHeight, 450);
            //    capture.Set(VideoCaptureProperties.FrameWidth, WindowWidth);
            //    capture.Set(VideoCaptureProperties.FrameHeight, WindowHeight);
            //    capture.Set(VideoCaptureProperties.Fps, frameRate);
            //}
            //else
            //{
            //    throw new Exception("Error opening video capture.");
            //}

            // Initialize VideoWriter to save frames to a video file
            //videoWriter = new VideoWriter("animation_output.avi", FourCC.XVID, frameRate, new OpenCvSharp.Size(width, height), true);

            //var font = HersheyFonts.HersheyComplex;
            //Cv2.PutText(_mat, "test", new OpenCvSharp.Point(50, 300), font, 3, new Scalar(254, 254, 254, 1), 3, LineTypes.AntiAlias);
            //Cv2.PutText()

            // Show the initial frame
            //UpdateImage();
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Create a 540x960 green Mat
            greenMat = new Mat(540, 960, MatType.CV_8UC3, new Scalar(0, 255, 0));
            MainWindow mWin = (MainWindow)Application.Current.MainWindow;
            // Initialize the OpenCvSharp Window
            //Cv2.ImShow("GreenMatWindow", greenMat);
            //Cv2.WaitKey(1);

            // Create the text block
            //textBlock = new TextBlock
            //{
            //    Text = "SUPER MARIO MAKER 2 BUDDY",
            //    FontSize = 40,
            //    Foreground = new SolidColorBrush(Colors.White),
            //    FontFamily = new System.Windows.Media.FontFamily("SUPER MARIO MAKER 2") // Set the desired font (Arial in this case)
            //};

            //// Add the text block to the WPF window
            //mainGrid.Children.Add(textBlock);

            //// Set the initial opacity to 0
            //textBlock.Opacity = 0;
            //if (mWin.ActiveLevel)
            //codeBlock.Text = "000-000-000";

            //// Create a DispatcherTimer for fading in
            //fadeTimer = new DispatcherTimer
            //{
            //    Interval = TimeSpan.FromMilliseconds(10)
            //};
            //fadeTimer.Tick += FadeInText;
            //fadeTimer.Start();
        }

        private void FadeInText(object sender, EventArgs e)
        {
            if (currentOpacity < 1.0)
            {
                currentOpacity += 0.20; // Increase the opacity increment for a faster fade-in effect
                if (currentOpacity > 1.0)
                    currentOpacity = 1.0; // Ensure opacity doesn't go beyond 1
                codeBlock.Opacity = currentOpacity;
                nameBlock.Opacity = currentOpacity;
                creatorBlock.Opacity = currentOpacity;
                timeBlock.Opacity = currentOpacity;
            }
            else
            {
                fadeTimer.Stop();
            }
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            MainWindow mWin = (MainWindow)Application.Current.MainWindow;
            Level lvl = mWin.ActiveLevel;

            if (mWin.OverlaySelection == "Custom")
            {
                //
                // Create custom overlay using user's settings
                // 
                if (lvl.Active)
                {
                    if (mWin.CodeSettings)
                        ProcessTextBlock(codeBlock, lvl.Code, mWin.FSizeCode, mWin.FontCode, mWin.XPosCode, mWin.YPosCode);
                    else
                        codeBlock.Visibility = Visibility.Hidden;

                    if (mWin.NameSettings)
                        ProcessTextBlock(nameBlock, lvl.Name, mWin.FSizeName, mWin.FontName, mWin.XPosName, mWin.YPosName);
                    else
                        nameBlock.Visibility = Visibility.Hidden;

                    if (mWin.CreatorSettings)
                        ProcessTextBlock(creatorBlock, lvl.Creator, mWin.FSizeCreator, mWin.FontCreator, mWin.XPosCreator, mWin.YPosCreator);
                    else
                        creatorBlock.Visibility = Visibility.Hidden;

                    if (mWin.TimeSettings)
                        ProcessTextBlock(timeBlock, mWin.elapsedTime.ToString(@"hh\:mm\:ss"), mWin.FSizeTime, mWin.FontTime, mWin.XPosTime, mWin.YPosTime);
                    else
                        timeBlock.Visibility = Visibility.Hidden;

                    // Create a DispatcherTimer for fading in
                    //fadeTimer = new DispatcherTimer
                    //{
                    //    Interval = TimeSpan.FromMilliseconds(10)
                    //};
                    //fadeTimer.Tick += FadeInText;
                    //fadeTimer.Start();
                    // Start a timer to update the text alpha value
                }
            }

            //UpdateImage();
        }

        /// <summary>
        /// This processes all the user parameters set for each piece of data to display
        /// </summary>
        /// <param name="textBlock">The textBlock to be processed</param>
        /// <param name="text">Text string to go in textBlock</param>
        /// <param name="fontSize">Font size of text</param>
        /// <param name="font">Font code for the text</param>
        /// <param name="x">X position on screen</param>
        /// <param name="y"> position on screen</param>
        private void ProcessTextBlock(TextBlock textBlock, string text, double fontSize, string font, int x, int y)
        {
            MainWindow mWin = (MainWindow)Application.Current.MainWindow;
            textBlock.Visibility = Visibility.Visible;

            textBlock.Text = text;
            textBlock.FontSize = fontSize > 0 ? fontSize : 1;

            // Convert the font type string to a FontFamily
            System.Windows.Media.FontFamily fontFamily = new System.Windows.Media.FontFamily(font);
            // Apply the FontFamily to the TextBlock
            textBlock.FontFamily = fontFamily;

            MoveTextBlock(textBlock, x, y);
        }

        private void MoveTextBlock(TextBlock textBlock, double x, double y)
        {
            // Set the Margin of the TextBlock to position it
            textBlock.Margin = new Thickness(x, y, 0, 0);
        }
        //private void UpdateImage()
        //{
        //    //Cv2.ImShow("ScreenOverlayWin", _mat);
        //    //videoWriter.Write(_mat);
        //    //var bitmapSource = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(_mat);
        //    var bitmapSource = BitmapSourceConverter.ToBitmapSource(_mat);
        //    streamOverlayOut.Source = bitmapSource;
        //}

        //private void FadeInText(object sender, EventArgs e)
        //{
        //    MainWindow mWin = (MainWindow)Application.Current.MainWindow;

        //    // Calculate the elapsed time
        //    TimeSpan elapsed = DateTime.Now - startTime;

        //    // Calculate the current alpha value based on the elapsed time
        //    textAlpha = Math.Min(1.0, elapsed.TotalMilliseconds / fadeDuration);

        //    // Create a copy of the canvas Mat to draw the text on
        //    Mat canvasCopy = _mat.Clone();

        //    // Create a font for the text
        //    //var font = HersheyFonts.HersheyComplex;
        //    //var font = HersheyFonts.

        //    //HersheyFonts.
        //    //var font = new Font("Arial", 24, FontStyle.Bold);
        //    //var font = new Font(System.Drawing.FontFamily.GetFamilies().First, 24, FontStyle.Bold);

        //    // Draw the text on a separate Mat
        //    var textMat = new Mat(540, 960, MatType.CV_8UC4, new Scalar(64, 177, 0, 255));
        //    //Cv2.ImShow("Test", textMat);
        //    if (mWin.OverlaySelection == "Custom")
        //    {
        //        if (mWin.CodeSettings)
        //        {
        //            var fontFamily = new System.Drawing.FontFamily("Arial");
        //            var font = new Font(fontFamily, 24);
        //            var textImage = new Bitmap(960, 540, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        //            var graphics = Graphics.FromImage(textImage);
        //            graphics.Clear(System.Drawing.Color.Green);

        //            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        //            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        //            graphics.DrawString("Hello, OpenCvSharp!", font, System.Drawing.Brushes.White, PointF.Empty);
        //            //graphics.DrawString()

        //            BitmapSource wpfBitmap = ConvertToBitmapSource(textImage);
        //            // Create a Mat from the text image
        //            //OpenCvSharp.Mat mat = BitmapSourceConverter.ToMat(wpfBitmap);
        //            Mat newMat = new Mat(540, 960, MatType.CV_8UC4, new Scalar(64, 177, 0, 255));

        //            //Cv2.CvtColor()
        //            Cv2.CvtColor(wpfBitmap.ToMat(), newMat, ColorConversionCodes.BGRA2RGBA);
        //            Cv2.ImShow("Canvas with Text", newMat);
        //            //mat.CopyTo(newMat);
        //            //Mat canvasMat = new Mat(800, 600, MatType.CV_8UC4, new Scalar(0,0,0,0));

        //            // Display the canvasMat or perform further operations

        //            Cv2.AddWeighted(newMat, textAlpha, _mat, 1.0 - textAlpha, 0, _mat);
        //            //Cv2.ImShow("test", canvasCopy);

        //            // Now, you can overlay this mat on your main canvas using OpenCvSharp functions
        //            //var canvasMat = new Mat(new Size(800, 600), MatType.CV_8UC3, Scalar.Black);
        //            //mat.CopyTo(_mat.SubMat(50, 300, 0, 100));

        //            //Cv2.PutText(textMat, mWin.ActiveLevel.Code, new OpenCvSharp.Point(mWin.XPosCode, mWin.YPosCode), font, mWin.FSizeCode, textColor, 3, LineTypes.AntiAlias);
        //        }
        //    }

        //    //textMat

        //    // Blend the textMat with the canvasCopy using the alpha value
        //    //Cv2.AddWeighted(mat, textAlpha, canvasCopy, 1.0 -  textAlpha, 0, canvasCopy);
        //    //Cv2.AddWeighted(textMat, textAlpha, canvasCopy, 1.0 -  textAlpha, 0, canvasCopy);
        //    //Cv2.AddWeighted()

        //    // Display the canvasCopy in your WPF application
        //    //Image.Source = canvasCopy.ToWriteableBitmap();

        //    // Check if the animation is complete
        //    //_mat = canvasCopy;

        //    if (textAlpha >= 1.0)
        //    {
        //        // Stop the timer
        //        CompositionTarget.Rendering -= FadeInText;
        //    }
        //}

        public static BitmapSource ConvertToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }
        }
        internal static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
            internal static extern bool DeleteObject(IntPtr hObject);
        }


        // Old 10/10/2023
        /*
        public ScreenOverlayWin()
        {
            int frameRate = 5;
            int width = 1920;
            int height = 1080;

            InitializeComponent();
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            // Create a blank window
            _mat.SetTo(new Scalar(64, 177, 0, 255)); // green screen color

            // Initialize the VideoCapture object
            capture = new VideoCapture();
            capture.Open(2); // Use 0 for the default camera, adjust accordingly if you have multiple cameras

            if (capture.IsOpened())
            {
                //capture.Set(VideoCaptureProperties.FrameWidth, 800);
                //capture.Set(VideoCaptureProperties.FrameHeight, 450);
                capture.Set(VideoCaptureProperties.FrameWidth, WindowWidth);
                capture.Set(VideoCaptureProperties.FrameHeight, WindowHeight);
                capture.Set(VideoCaptureProperties.Fps, frameRate);
            }
            else
            {
                throw new Exception("Error opening video capture.");
            }

            // Initialize VideoWriter to save frames to a video file
            //videoWriter = new VideoWriter("animation_output.avi", FourCC.XVID, frameRate, new OpenCvSharp.Size(width, height), true);

            //var font = HersheyFonts.HersheyComplex;
            //Cv2.PutText(_mat, "test", new OpenCvSharp.Point(50, 300), font, 3, new Scalar(254, 254, 254, 1), 3, LineTypes.AntiAlias);
            //Cv2.PutText()

            // Show the initial frame
            UpdateImage();
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            MainWindow mWin = (MainWindow)Application.Current.MainWindow;

            if (mWin.OverlaySelection == "Custom")
            {
                //
                // Create custom overlay using user's settings
                // 
                if (mWin.ActiveLevel.Active)
                {
                
                    if (!isCodeFadingIn)
                    {
                        isCodeFadingIn = true;
                        startTime = DateTime.Now;
                        CompositionTarget.Rendering += FadeInText;
                    }
                    //startTime = DateTime.Now;

                    // Start a timer to update the text alpha value
                }
            }

            UpdateImage();
        }
        private void UpdateImage()
        {
            //Cv2.ImShow("ScreenOverlayWin", _mat);
            //videoWriter.Write(_mat);
            //var bitmapSource = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(_mat);
            var bitmapSource = BitmapSourceConverter.ToBitmapSource(_mat);
            streamOverlayOut.Source = bitmapSource;
        }

        private void FadeInText(object sender, EventArgs e)
        {
            MainWindow mWin = (MainWindow)Application.Current.MainWindow;

            // Calculate the elapsed time
            TimeSpan elapsed = DateTime.Now - startTime;

            // Calculate the current alpha value based on the elapsed time
            textAlpha = Math.Min(1.0, elapsed.TotalMilliseconds / fadeDuration);

            // Create a copy of the canvas Mat to draw the text on
            Mat canvasCopy = _mat.Clone();

            // Create a font for the text
            //var font = HersheyFonts.HersheyComplex;
            //var font = HersheyFonts.

            //HersheyFonts.
            //var font = new Font("Arial", 24, FontStyle.Bold);
            //var font = new Font(System.Drawing.FontFamily.GetFamilies().First, 24, FontStyle.Bold);

            // Draw the text on a separate Mat
            var textMat = new Mat(540, 960, MatType.CV_8UC4, new Scalar(64, 177, 0, 255));
            //Cv2.ImShow("Test", textMat);
            if (mWin.OverlaySelection == "Custom")
            {
                if (mWin.CodeSettings)
                {
                    var fontFamily = new System.Drawing.FontFamily("Arial");
                    var font = new Font(fontFamily, 24);
                    var textImage = new Bitmap(960, 540, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    var graphics = Graphics.FromImage(textImage);
                    graphics.Clear(System.Drawing.Color.Green);

                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    graphics.DrawString("Hello, OpenCvSharp!", font, System.Drawing.Brushes.White, PointF.Empty);
                    //graphics.DrawString()

                    BitmapSource wpfBitmap = ConvertToBitmapSource(textImage);
                    // Create a Mat from the text image
                    //OpenCvSharp.Mat mat = BitmapSourceConverter.ToMat(wpfBitmap);
                    Mat newMat = new Mat(540, 960, MatType.CV_8UC4, new Scalar(64, 177, 0, 255));

                    //Cv2.CvtColor()
                    Cv2.CvtColor(wpfBitmap.ToMat(), newMat, ColorConversionCodes.BGRA2RGBA);
                    Cv2.ImShow("Canvas with Text", newMat);
                    //mat.CopyTo(newMat);
                    //Mat canvasMat = new Mat(800, 600, MatType.CV_8UC4, new Scalar(0,0,0,0));

                    // Display the canvasMat or perform further operations

                    Cv2.AddWeighted(newMat, textAlpha, _mat, 1.0 - textAlpha, 0, _mat);
                    //Cv2.ImShow("test", canvasCopy);

                    // Now, you can overlay this mat on your main canvas using OpenCvSharp functions
                    //var canvasMat = new Mat(new Size(800, 600), MatType.CV_8UC3, Scalar.Black);
                    //mat.CopyTo(_mat.SubMat(50, 300, 0, 100));

                    //Cv2.PutText(textMat, mWin.ActiveLevel.Code, new OpenCvSharp.Point(mWin.XPosCode, mWin.YPosCode), font, mWin.FSizeCode, textColor, 3, LineTypes.AntiAlias);
                }
            }

            //textMat

            // Blend the textMat with the canvasCopy using the alpha value
            //Cv2.AddWeighted(mat, textAlpha, canvasCopy, 1.0 -  textAlpha, 0, canvasCopy);
            //Cv2.AddWeighted(textMat, textAlpha, canvasCopy, 1.0 -  textAlpha, 0, canvasCopy);
            //Cv2.AddWeighted()

            // Display the canvasCopy in your WPF application
            //Image.Source = canvasCopy.ToWriteableBitmap();

            // Check if the animation is complete
            //_mat = canvasCopy;

            if (textAlpha >= 1.0)
            {
                // Stop the timer
                CompositionTarget.Rendering -= FadeInText;
            }
        }
         */

        //private void FadeInText(object sender, EventArgs e)
        //{
        //    // Calculate the elapsed time
        //    TimeSpan elapsed = DateTime.Now - startTime;

        //    // Calculate the current alpha value based on the elapsed time
        //    textAlpha = (int)(255 * (elapsed.TotalMilliseconds / fadeDuration));

        //    // Cap the alpha value to 255 to avoid exceeding it
        //    textAlpha = Math.Min(textAlpha, 255);

        //    // Create a copy of the canvas Mat to draw the text on

        //    // Create a font for the text
        //    var font = HersheyFonts.HersheyComplex;

        //    // Draw the text with the current alpha value on the canvasCopy
        //    Cv2.PutText(_mat, textToDisplay, new OpenCvSharp.Point(50, 300), font, 3, new Scalar(textColor.Val0, textColor.Val1, textColor.Val2, textAlpha), 3, LineTypes.AntiAlias);


        //    // Check if the animation is complete
        //    if (textAlpha >= 255)
        //    {
        //        // Stop the timer
        //        CompositionTarget.Rendering -= FadeInText;
        //    }
        //}
    }
}



        //
        // Old ball bouncing animation
        //
        /*
        public partial class ScreenOverlayWin : System.Windows.Window
        {
            private VideoCapture capture;
            //private VideoWriter videoWriter;

            private const int WindowWidth = 1920;
            private const int WindowHeight = 1080;
            private const int BallRadius = 50;



            private readonly Scalar _ballColor = new Scalar(0, 0, 255); // Red color
            private readonly Random _random = new Random();
            private readonly Mat _mat = new Mat(WindowHeight, WindowWidth, MatType.CV_8UC3);

            private int _ballX;
            private int _ballY;
            private int _ballSpeedX;
            private int _ballSpeedY;
            public ScreenOverlayWin()
            {
                int frameRate = 30;
                int width = 1920;
                int height = 1080;

                InitializeComponent();
                CompositionTarget.Rendering += CompositionTarget_Rendering;

                _ballX = WindowWidth / 2;
                _ballY = WindowHeight / 2;
                _ballSpeedX = _random.Next(-5, 6); // Random initial speed between -5 and 5
                _ballSpeedY = _random.Next(-5, 6);

                // Create a blank window
                _mat.SetTo(new Scalar(0, 0, 0)); // Transparent black

                // Initialize the VideoCapture object
                capture = new VideoCapture();
                capture.Open(2); // Use 0 for the default camera, adjust accordingly if you have multiple cameras

                if (capture.IsOpened())
                {
                    capture.Set(VideoCaptureProperties.FrameWidth, 800);
                    capture.Set(VideoCaptureProperties.FrameHeight, 450);
                    capture.Set(VideoCaptureProperties.Fps, frameRate);
                }
                else
                {
                    throw new Exception("Error opening video capture.");
                }

                // Initialize VideoWriter to save frames to a video file
                //videoWriter = new VideoWriter("animation_output.avi", FourCC.XVID, frameRate, new OpenCvSharp.Size(width, height), true);

                // Show the initial frame
                UpdateImage();
            }

            private void CompositionTarget_Rendering(object sender, EventArgs e)
            {
                // Update ball position
                _ballX += _ballSpeedX;
                _ballY += _ballSpeedY;

                // Check boundaries and make the ball bounce
                if (_ballX - BallRadius < 0 || _ballX + BallRadius >= WindowWidth)
                {
                    _ballSpeedX *= -1;
                }

                if (_ballY - BallRadius < 0 || _ballY + BallRadius >= WindowHeight)
                {
                    _ballSpeedY *= -1;
                }

                //// Clear the frame
                //_mat.SetTo(new Scalar(0, 0, 0, 0));
                // Clear the frame
                _mat.SetTo(Scalar.All(0)); // Black color

                // Draw the ball
                Cv2.Circle(_mat, new OpenCvSharp.Point(_ballX, _ballY), BallRadius, _ballColor, -1, LineTypes.AntiAlias);

                // Update the image
                UpdateImage();
            }
            private void UpdateImage()
            {
                //Cv2.ImShow("ScreenOverlayWin", _mat);
                //videoWriter.Write(_mat);
                //var bitmapSource = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(_mat);
                var bitmapSource = BitmapSourceConverter.ToBitmapSource(_mat);
                streamOverlayOut.Source = bitmapSource;
            }
            */

        //private void Timer_Tick(object sender, EventArgs e)
        //{
        //    // Update ball position
        //    ballX += ballSpeedX;
        //    ballY += ballSpeedY;

        //    // Reverse ball direction if it hits the window boundaries
        //    if (ballX < 0 || ballX + ballSize > (int)Width)
        //        ballSpeedX = -ballSpeedX;
        //    if (ballY < 0 || ballY + ballSize > (int)Height)
        //        ballSpeedY = -ballSpeedY;

        //    // Create a new BitmapSource
        //    bitmapSource = new WriteableBitmap((int)Width, (int)Height, 96, 96, PixelFormats.Pbgra32, null);

        //    // Get the pixel buffer from the BitmapSource and modify the pixels to draw the red ball
        //    int stride = bitmapSource.PixelWidth * (bitmapSource.Format.BitsPerPixel / 8);
        //    byte[] pixelBuffer = new byte[bitmapSource.PixelHeight * stride];
        //    bitmapSource.CopyPixels(pixelBuffer, stride, 0);

        //    for (int y = 0; y < bitmapSource.PixelHeight; y++)
        //    {
        //        for (int x = 0; x < bitmapSource.PixelWidth; x++)
        //        {
        //            int index = y * stride + x * 4;

        //            // Check if the current pixel is within the bounds of the ball
        //            if (x >= ballX && x < ballX + ballSize && y >= ballY && y < ballY + ballSize)
        //            {
        //                pixelBuffer[index] = 255;   // Set red channel to maximum
        //                pixelBuffer[index + 1] = 0; // Set green channel to minimum
        //                pixelBuffer[index + 2] = 0; // Set blue channel to minimum
        //                pixelBuffer[index + 3] = 255; // Set alpha channel to maximum
        //            }
        //            else
        //            {
        //                // Preserve original pixel values
        //                pixelBuffer[index] = 0;
        //                pixelBuffer[index + 1] = 0;
        //                pixelBuffer[index + 2] = 0;
        //                pixelBuffer[index + 3] = 0;
        //            }
        //        }
        //    }

        //    // Write the modified pixel buffer back to the BitmapSource
        //    //bitmapSource.
        //    //bitmapSource.CopyPixels(new Int32Rect(0, 0, bitmapSource.PixelWidth, bitmapSource.PixelHeight), pixelBuffer, stride, 0);
        //    bitmapSource.WritePixels(new Int32Rect(0, 0, bitmapSource.PixelWidth, bitmapSource.PixelHeight), pixelBuffer, stride, 0);

        //    // Update the Image control's Source property
        //    streamOverlayOut.Source = bitmapSource;
        //}
    //}
//}
