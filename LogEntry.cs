﻿using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MM2Buddy
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            // Customize the string representation as needed
            return $"[{Timestamp}] {Message}";
        }
    }
}
