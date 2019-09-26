using System.Collections.Generic;

namespace localStar.Connection
{
    public class Map<T1, T2>
    {
        private SortedDictionary<T1, T2> _forward = new SortedDictionary<T1, T2>();
        private SortedDictionary<T2, T1> _reverse = new SortedDictionary<T2, T1>();

        public SortedDictionary<T1, T2> Forward { get => _forward; }
        public SortedDictionary<T2, T1> Backward { get => _reverse; }


        public Map()
        {
        }

        public class Indexer<T3, T4>
        {
            private SortedDictionary<T3, T4> _dictionary;
            public Indexer(SortedDictionary<T3, T4> dictionary)
            {
                _dictionary = dictionary;
            }
            public T4 this[T3 index]
            {
                get
                {
                    T4 tmp;
                    if (_dictionary.TryGetValue(index, out tmp))
                        return tmp;
                    else
                        return default(T4);
                }
                set { _dictionary[index] = value; }
            }
        }

        public void Add(T1 t1, T2 t2)
        {
            _forward.Add(t1, t2);
            _reverse.Add(t2, t1);
        }
        public void RemoveForward(T1 t1)
        {
            if (!_forward.ContainsKey(t1)) return;
            _reverse.Remove(_forward[t1]);
            _forward.Remove(t1);
        }
        public void RemoveBackward(T2 t2)
        {
            if (!_reverse.ContainsKey(t2)) return;
            _forward.Remove(_reverse[t2]);
            _reverse.Remove(t2);
        }

    }
}