using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace MM2Buddy
{
    public class CircularBuffer<T>
    {
        private T[] buffer;
        private int head;
        private int tail;

        public CircularBuffer(int capacity)
        {
            buffer = new T[capacity];
            head = 0;
            tail = 0;
        }

        public int Count => tail <= head ? head - tail : buffer.Length - (tail - head);

        public void Add(T item)
        {
            buffer[head] = item;
            head = (head + 1) % buffer.Length;
            if (head == tail)
            {
                tail = (tail + 1) % buffer.Length;
            }

            // Append the log entry to the file immediately
            LogToTextFile(item);
        }

        public IEnumerable<T> GetAll()
        {
            for (int i = tail; i != head; i = (i + 1) % buffer.Length)
            {
                yield return buffer[i];
            }
        }

        private void LogToTextFile(T logEntry)
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                var logFilePath = System.IO.Path.Combine(appDataPath, "MM2Buddy", "MM2DataLog.txt");

                // Create the file if it doesn't exist
                //if (!File.Exists(logFilePath))
                //{
                //    using (File.Create(logFilePath)) { }
                //}

                // Append the log entry to the file
                File.AppendAllText(logFilePath, logEntry.ToString() + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Handle the exception, e.g., display an error message
                CustomMessageBox customMessageBox = new CustomMessageBox($"Error logging to file: {ex.Message}");
                customMessageBox.Show();
            }
        }
    }
}
