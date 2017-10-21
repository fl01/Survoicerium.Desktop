using System.Collections;
using System.Collections.Generic;

namespace Survoicerium.Client.Common
{
    public class FixedSizeQueue<T> : IEnumerable<T>
    {
        private Queue<T> _queue = new Queue<T>();

        public int Count { get; set; }

        public FixedSizeQueue(int count)
        {
            Count = count;
        }

        public void Enqueue(T item)
        {
            if (_queue.Count >= Count)
            {
                _queue.Dequeue();
            }

            _queue.Enqueue(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _queue.GetEnumerator();
        }
    }
}
