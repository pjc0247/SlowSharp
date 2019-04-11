using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slowsharp
{
    internal class PointableDictionary<TKey, TValue> // : IDictionary<TKey, TValue>
    {
        private Dictionary<TKey, int> keys = new Dictionary<TKey, int>();
        private List<TValue> values = new List<TValue>();
        private int nextPtr = 0;

        // Does not provide indexer property
        //   can be ambiguous and not fit to purpose of this container
        //public TValue this[TKey key]

        public int Add(TKey key, TValue value)
        {
            int ptr = 0;
            if (keys.TryGetValue(key, out ptr))
            {
                values[ptr] = value;
                return ptr;
            }
            else
            {
                keys[key] = nextPtr;
                values.Add(value);
                return nextPtr++;
            }
        }
        public TValue Get(TKey key)
        {
            int ptr = 0;
            if (keys.TryGetValue(key, out ptr))
                return values[ptr];
            throw new KeyNotFoundException($"{key}");
        }
        public int GetPtr(TKey key)
        {
            int ptr = 0;
            if (keys.TryGetValue(key, out ptr))
                return ptr;
            return -1;
        }
        public bool ContainsKey(TKey key)
            => GetPtr(key) != -1;

        public TValue GetByPtr(int ptr)
            => values[ptr];
        public void SetByPtr(int ptr, TValue value)
            => values[ptr] = value;
    }
}
