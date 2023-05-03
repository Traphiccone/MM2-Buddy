using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenCvSharp.LineIterator;

namespace MM2Buddy
{
    internal class PixelColorCheck
    {
        public int X { get; set; }
        public int Y { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public PixelColorCheck(int x, int y, byte r, byte g, byte b)
        {
            X = x;
            Y = y;
            R = r;
            G = g;
            B = b;
        }

        public double CompareColor(PixelColorCheck otherPoint)
        {
            double rDiff = Math.Abs(this.R - otherPoint.R);
            double gDiff = Math.Abs(this.G - otherPoint.G);
            double bDiff = Math.Abs(this.B - otherPoint.B);

            double totalDiff = rDiff + gDiff + bDiff;
            double avgDiff = totalDiff / 3.0;

            double similarity = 1.0 - (avgDiff / 255.0);
            return similarity * 100.0;
        }
    }
}
