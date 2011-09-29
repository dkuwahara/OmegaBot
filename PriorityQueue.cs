using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BattleNet
{
    class PriorityQueue<P, V>
    {
        private ReaderWriterLockSlim queueLock = new ReaderWriterLockSlim();
        private SortedDictionary<P, Queue<V>> list = new SortedDictionary<P, Queue<V>>();
        public void Enqueue(P priority, V value)
        {
            queueLock.EnterWriteLock();
            try
            {
                Queue<V> q;
                if (!list.TryGetValue(priority, out q))
                {
                    q = new Queue<V>();
                    list.Add(priority, q);
                }
                q.Enqueue(value);
            }
            finally
            {
                queueLock.ExitWriteLock();
            }
        }
        public void Clear()
        {
            list.Clear();
        }
        public V Dequeue()
        {   
            V v;
            KeyValuePair<P,Queue<V>> pair;
            queueLock.EnterWriteLock();
            try
            {
                // will throw if there isn’t any first element!
                pair = list.First();
                v = pair.Value.Dequeue();
                if (pair.Value.Count == 0) // nothing left of the top priority.
                    list.Remove(pair.Key);
            }
            finally
            {
                queueLock.ExitWriteLock();
            }
            return v;
        }
        public bool IsEmpty()
        {
            bool empty = true;
            queueLock.EnterReadLock();
            try
            {
                empty = !list.Any();
            }
            finally
            {
                queueLock.ExitReadLock();
            }
            return empty;
        }
    }
}
