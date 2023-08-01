using Microsoft.Office.Interop.Excel;
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
//using OpenCvSharp.Extensions;

namespace MM2Buddy
{
    /// <summary>
    /// Interaction logic for ScreenOverlayWin.xaml
    /// </summary>
    public partial class ScreenOverlayWin : System.Windows.Window
    {
        private VideoCapture capture;
        private VideoWriter videoWriter;

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
            videoWriter = new VideoWriter("animation_output.avi", FourCC.XVID, frameRate, new OpenCvSharp.Size(width, height), true);

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
    }
}
