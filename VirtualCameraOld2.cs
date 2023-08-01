using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media;
using static Emgu.Util.Platform;
using System.Drawing;
using Emgu.CV;
using System.Net;
using System.Windows;

namespace MM2Buddy
{
    public class VirtualCameraOld2
    {
        private OpenCvSharp.VideoWriter _writer = new OpenCvSharp.VideoWriter("output.mpeg", OpenCvSharp.VideoWriter.FourCC('M', 'J', 'P', 'G'), 30, new OpenCvSharp.Size(640, 480));
        private const int WindowWidth = 640;
        private const int WindowHeight = 480;
        private const int BallRadius = 50;

        private readonly Scalar _ballColor = new Scalar(0, 0, 255); // Red color
        private readonly Random _random = new Random();
        private readonly OpenCvSharp.Mat _mat = new OpenCvSharp.Mat(WindowHeight, WindowWidth, MatType.CV_8UC3);

        private int _ballX;
        private int _ballY;
        private int _ballSpeedX;
        private int _ballSpeedY;
        // HTTP server prefix (change the port if needed)
        static string serverPrefix = "http://localhost:8000/";
        static string imageFilePath = "output_image.png";
        // CancellationTokenSource and ManualResetEvent to handle stopping the HTTP listener gracefully
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        static ManualResetEventSlim stopEvent = new ManualResetEventSlim();

        public VirtualCameraOld2()
        {
            //InitializeComponent();
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            // Create a new video writer.
            //_writer = new VideoWriter("output.avi", VideoWriter.FourCC('M', 'J', 'P', 'G'), 30, new Size(640, 480));
            _ballX = WindowWidth / 2;
            _ballY = WindowHeight / 2;
            _ballSpeedX = _random.Next(-5, 6); // Random initial speed between -5 and 5
            _ballSpeedY = _random.Next(-5, 6);

            // Start a new thread to constantly update the video file.
            //Thread thread = new Thread(() =>
            //{
            //    while (true)
            //    {
            //        // Create a new Mat object.
            //        Mat frame = CreateFrame();
            //        Cv2.ImShow("Large View", frame);

            //        // Write the frame to the video file.
            //        _writer.Write(frame);

            //        // Write the frame to a PNG file.
            //        //SaveFrameToPNG(_mat, "frame.png");
            //    }
            //});
            //thread.Start();

            // Create and start the HTTP listener
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(serverPrefix);
            listener.Start();

            Console.WriteLine("HTTP server started. Listening for requests...");

            // Start a separate thread to handle incoming HTTP requests
            ThreadPool.QueueUserWorkItem(HandleRequests, listener);

            // Simulate continuous updates to the dynamic image every 2 seconds (replace this with your logic)
            // In your application, you should update the image file whenever needed.
            Timer updateTimer = new Timer(UpdateDynamicImage, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

            // Wait for the user to exit the application
            //Console.WriteLine("Press any key to exit.");
            //Console.ReadKey();
            cancellationTokenSource.Cancel();
            stopEvent.Wait();

            // Stop the HTTP listener when the user exits
            listener.Stop();
        }

        // Method to update the dynamic image (replace this with your dynamic image update logic)
        public void UpdateDynamicImage(object state)
        {
            // Simulate updating the image by creating a random byte array and saving it to the file
            //Random random = new Random();
            //byte[] randomImageBytes = new byte[1024];
            //random.NextBytes(randomImageBytes);

            //File.WriteAllBytes(imageFilePath, randomImageBytes);
            //CompositionTarget_Rendering();
            //Console.WriteLine("Dynamic image updated.");
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
            //_writer.Write(_mat);

            string outputPath = "output_image.png";
            _mat.SaveImage(outputPath);

            Cv2.ImShow("Large View", _mat);
        }

        // Method to handle incoming HTTP requests
        static void HandleRequests(object state)
        {
            HttpListener listener = (HttpListener)state;

            while (listener.IsListening)
            {
                try
                {
                    // Wait for an incoming request
                    HttpListenerContext context = listener.GetContext();

                    // Get the response object to send the dynamic image
                    HttpListenerResponse response = context.Response;

                    // Read the dynamic image file and send it as the HTTP response
                    byte[] imageBytes = File.ReadAllBytes(imageFilePath);

                    response.ContentType = "image/png";
                    response.ContentLength64 = imageBytes.Length;

                    using (Stream output = response.OutputStream)
                    {
                        output.Write(imageBytes, 0, imageBytes.Length);
                    }

                    // Close the response to send it to the client
                    response.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error handling request: " + ex.Message);
                    // Handle any errors here (logging, etc.)
                }
            }
        }

        private static void SaveFrameToPNG(OpenCvSharp.Mat frame, string filename)
        {
            //// Save the frame to a PNG file.
            //var bitmapSource = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(frame);
            ////System.Drawing.Image bmp = frame.;
            //bitmapSource.Save(filename);
        }

        //private static Mat CreateFrame()
        //{
        //    // Create a new Mat object with an alpha channel.
        //    //Mat frame = new Mat(480, 640, MatType.CV_8UC3);


        //    // Fill the Mat object with a random color.
        //    //Random random = new Random();
        //    //for (int x = 0; x < 479; x++)
        //    //{
        //    //    for (int y = 0; y < frame.Height; y++)
        //    //    {
        //    //        frame.Set(x, y, new Scalar(random.Next(255), random.Next(255), random.Next(255)));
        //    //    }
        //    //}
        //    //return frame;
        //}

        public OpenCvSharp.Mat CreateFrame()
        {
            // Create a new Mat object with an alpha channel.
            //Mat frame = new Mat(480, 640, MatType.CV_8UC3);

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
            //Cv2.ImShow("Large View", _mat);
            // Update the image
            //_writer.Write(_mat);
            // Fill the Mat object with a random color.
            //Random random = new Random();
            //for (int x = 0; x < 479; x++)
            //{
            //    for (int y = 0; y < frame.Height; y++)
            //    {
            //        frame.Set(x, y, new Scalar(random.Next(255), random.Next(255), random.Next(255)));
            //    }
            //}
            return _mat;
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
            //_writer.Write(_mat);

            string outputPath = "output_image.png";
            _mat.SaveImage(outputPath);

            Cv2.ImShow("Large View", _mat);
            // Update the image

            //UpdateImage();
        }

        //public void UseAsVirtualCamera()
        //{
        //    // Use the video writer as a virtual camera.
        //    VideoCapture capture = new VideoCapture(_writer);

        //    // Select the virtual camera in OBS.
        //    OBS.AddVirtualCamera(capture);
        //}

        //public void UseAsVirtualCamera()
        //{
        //    // Open the video file in a video player.
        //    VideoPlayer player = new VideoPlayer("output.avi");

        //    // Use the video player as your virtual camera in other applications.
        //    player.UseAsVirtualCamera();
        //}
    }
}
