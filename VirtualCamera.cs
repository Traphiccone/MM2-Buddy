using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using OpenCvSharp;

namespace MM2Buddy
{
    public class VirtualCamera
    {
        private VideoCapture capture;
        private VideoWriter videoWriter;
        private Thread animationThread;
        private bool isAnimationRunning;

        public VirtualCamera()
        {
            // Set the capture resolution and frame rate to match your animation
            int width = 1280;
            int height = 720;
            int frameRate = 30;

            // Initialize the VideoCapture object
            capture = new VideoCapture();
            capture.Open(2); // Use 0 for the default camera, adjust accordingly if you have multiple cameras

            if (capture.IsOpened())
            {
                capture.Set(VideoCaptureProperties.FrameWidth, width);
                capture.Set(VideoCaptureProperties.FrameHeight, height);
                capture.Set(VideoCaptureProperties.Fps, frameRate);
            }
            else
            {
                throw new Exception("Error opening video capture.");
            }

            // Initialize VideoWriter to save frames to a video file
            videoWriter = new VideoWriter("animation_output.avi", FourCC.XVID, frameRate, new Size(width, height), true);
        }

        public void StartAnimation()
        {
            if (!isAnimationRunning)
            {
                isAnimationRunning = true;
                animationThread = new Thread(AnimationLoop);
                animationThread.Start();
            }
        }

        public void StopAnimation()
        {
            if (isAnimationRunning)
            {
                isAnimationRunning = false;
                animationThread.Join();
            }
        }

        private void AnimationLoop()
        {
            using (Mat frame = new OpenCvSharp.Mat(1280, 720, MatType.CV_8UC3))
            {
                while (isAnimationRunning)
                {
                    // Generate your dynamic animation frame using OpenCvSharp
                    // Replace this part with your actual animation generation code
                    frame.SetTo(new Scalar(255, 0, 0)); // Set the frame to a blue color as an example

                    Cv2.ImShow("Large View", frame);

                    // Write the frame to the video file
                    videoWriter.Write(frame);

                    // Sleep to control the frame rate (assuming the animation is meant to be played at a specific frame rate)
                    Thread.Sleep(1000 / 30); // 30 FPS as an example, adjust as needed
                }
            }
        }

        public void Dispose()
        {
            if (isAnimationRunning)
            {
                isAnimationRunning = false;
                animationThread.Join();
            }

            capture.Release();
            capture.Dispose();
        }
    }
}
