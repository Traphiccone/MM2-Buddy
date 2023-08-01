using DirectShowLib;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MM2Buddy
{
    public class VirtualCameraOld
    {
        private IFilterGraph2 graphBuilder;
        private IBaseFilter virtualCamera;
        private IBaseFilter smartTee;
        private IBaseFilter sampleGrabber;
        private IPin virtualCameraOutputPin;
        private IPin sampleGrabberInputPin;

        public VirtualCameraOld()
        {
            graphBuilder = (IFilterGraph2)new FilterGraph();
            virtualCamera = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("{860BB310-5D01-11D0-BD3B-00A0C911CE86}")));
            smartTee = (IBaseFilter)new SmartTee();
            sampleGrabber = (IBaseFilter)new SampleGrabber();

            graphBuilder.AddFilter(virtualCamera, "MM2Buddy Virtual Cam");
            graphBuilder.AddFilter(smartTee, "Smart Tee");
            graphBuilder.AddFilter(sampleGrabber, "Sample Grabber");

            // Connect the virtual camera to the smart tee
            var virtualCameraOutputPin = DsFindPin.ByDirection(virtualCamera, PinDirection.Output, 0);
            var smartTeeInputPin = DsFindPin.ByDirection(smartTee, PinDirection.Input, 0);
            graphBuilder.Connect(virtualCameraOutputPin, smartTeeInputPin);

            // Connect the smart tee to the sample grabber
            var smartTeeOutputPin = DsFindPin.ByDirection(smartTee, PinDirection.Output, 0);
            var sampleGrabberInputPin = DsFindPin.ByDirection(sampleGrabber, PinDirection.Input, 0);
            graphBuilder.Connect(smartTeeOutputPin, sampleGrabberInputPin);

            // Capture frames from the OpenCV Mat and feed them to the sample grabber
            Task.Run(() =>
            {
                while (true)
                {
                    // Replace this with your actual dynamic video generation logic
                    using (var frame = GenerateDynamicFrame())
                    {
                        var imgData = frame.ToBytes();
                        ((ISampleGrabberCB)sampleGrabber).BufferCB(0, Marshal.UnsafeAddrOfPinnedArrayElement(imgData, 0), imgData.Length);
                    }
                }
            });

            var mediaControl = (IMediaControl)graphBuilder;
            mediaControl.Run();
        }

        private Mat GenerateDynamicFrame()
        {
            // Replace this with your actual dynamic video generation logic
            // For example, you can draw the animation, and create a Mat representing the frame.
            // The size and type of the Mat should match the virtual camera's expected input.
            var frame = new Mat(480, 640, MatType.CV_8UC3, Scalar.All(0));
            Cv2.Circle(frame, new Point(320, 240), 50, Scalar.Red, -1);
            Cv2.ImShow("Test", frame);
            return frame;
        }

        public void Start()
        {
            var cameraControl = (IAMCameraControl)virtualCamera;
            cameraControl.Set(CameraControlProperty.Focus, 0, CameraControlFlags.Auto);
        }

        public void Stop()
        {
            var mediaControl = (IMediaControl)graphBuilder;
            mediaControl.StopWhenReady();
        }
    }
}
