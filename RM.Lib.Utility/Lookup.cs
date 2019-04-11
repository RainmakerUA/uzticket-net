using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RM.Lib.Utility
{
    public sealed class Lookup<TKey, TValue> : ILookup<TKey, TValue>
    {
        private sealed class Grouping : IGrouping<TKey, TValue>
        {
            private readonly KeyValuePair<TKey, List<TValue>> _pair;

            public Grouping(KeyValuePair<TKey, List<TValue>> pair)
            {
                _pair = pair;
            }

            public TKey Key => _pair.Key;

            public List<TValue> Value => _pair.Value;

            public IEnumerator<TValue> GetEnumerator()
            {
                return _pair.Value.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private readonly Dictionary<TKey, List<TValue>> _dict = new Dictionary<TKey, List<TValue>>();

        public int Count => _dict.Count;

        public IEnumerable<TValue> this[TKey key] => _dict[key];

        public bool Contains(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator()
        {
            return _dict.Select(kv => new Grouping(kv)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TKey key, TValue value)
        {
            if (!_dict.TryGetValue(key, out var keyList))
            {
                keyList = new List<TValue>();
                _dict.Add(key, keyList);
            }

            keyList.Add(value);
        }
    }
}
