using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        public IEnumerable<T> GetAll()
        {
            for (int i = tail; i != head; i = (i + 1) % buffer.Length)
            {
                yield return buffer[i];
            }
        }
    }
}
