#region Using

using System;
using System.Collections.Generic;

#endregion

namespace PSSMDemo
{
    /// <summary>
    /// 構造体の管理に特化したリストの実装です。
    /// </summary>
    /// <remarks>
    /// IEnumerable.GetEnumerator() について、一般的なコレクション クラスは構造体を返却するため、
    /// クラス オブジェクトの生成は発生しませんが、
    /// IEnumerator インタフェースとして扱う場合にはボクシングが発生します。
    /// このため、このクラスでは IEnumerable を実装せずに、
    /// 明示的なインデックスによるアクセスを前提としています。
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public sealed class StructList<T> where T : struct
    {
        public delegate void RefAction<TValue>(ref TValue value);

        public delegate bool RefPredicate<TValue>(ref TValue value);

        // List のデフォルト値と同じ。
        const int defaultCapacity = 4;

        readonly T[] emptyArray = new T[0];

        T[] items;

        int size;

        /// <summary>
        /// 容量を取得または設定します。
        /// </summary>
        public int Capacity
        {
            get
            {
                return items.Length;
            }
            set
            {
                if (value < size) throw new ArgumentOutOfRangeException("value");

                if (value != items.Length)
                {
                    if (0 < value)
                    {
                        T[] newItems = new T[value];
                        if (size > 0) Array.Copy(items, 0, newItems, 0, size);
                        items = newItems;
                    }
                    else
                    {
                        items = emptyArray;
                    }
                }
            }
        }

        public int Count
        {
            get { return size; }
        }

        public T this[int index]
        {
            get
            {
                T result;
                GetItem(index, out result);
                return result;
            }
            set
            {
                SetItem(index, ref value);
            }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <remarks>
        /// 初期状態ではサイズ 0 の配列を用います。
        /// 要素の追加が行われた場合に初期容量としてサイズ 4 の配列を確保します。
        /// </remarks>
        public StructList()
        {
            items = emptyArray;
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <param name="capacity">初期容量。</param>
        public StructList(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity");

            items = new T[capacity];
        }

        public void SetItem(int index, ref T item)
        {
            // List<T> の実装より。
            // uint キャストで負値を転じさせ、0 未満判定を削減。
            if ((uint) size <= (uint) index) throw new ArgumentOutOfRangeException("index");

            items[index] = item;
        }

        public void GetItem(int index, out T result)
        {
            // List<T> の実装より。
            // uint キャストで負値を転じさせ、0 未満判定を削減。
            if ((uint) size <= (uint) index) throw new ArgumentOutOfRangeException("index");

            result = items[index];
        }

        public void GetLastItem(out T result)
        {
            GetItem(Count - 1, out result);
        }

        public int IndexOf(T item)
        {
            return IndexOf(item);
        }

        public int IndexOf(ref T item)
        {
            return Array.IndexOf(items, item, 0, size);
        }

        public int IndexOf(Predicate<T> predicate)
        {
            if (predicate == null) throw new ArgumentNullException("predicate");

            for (int i = 0; i < size; i++)
            {
                if (predicate(items[i]))
                    return i;
            }

            return -1;
        }

        public int IndexOf(RefPredicate<T> predicate)
        {
            if (predicate == null) throw new ArgumentNullException("predicate");

            for (int i = 0; i < size; i++)
            {
                if (predicate(ref items[i]))
                    return i;
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            Insert(index, ref item);
        }

        public void Insert(int index, ref T item)
        {
            if ((uint) size < (uint) index) throw new ArgumentOutOfRangeException("index");

            if (size == items.Length) EnsureCapacity(size + 1);
            if (index < size)
            {
                Array.Copy(items, index, items, index + 1, size - index);
            }
            items[index] = item;
            size++;
        }

        public void RemoveAt(int index)
        {
            if ((uint) size <= (uint) index) throw new ArgumentOutOfRangeException("index");

            size--;
            if (index < size)
            {
                Array.Copy(items, index + 1, items, index, size - index);
            }
            items[size] = default(T);
        }

        public void Add(T item)
        {
            Add(ref item);
        }

        public void Add(ref T item)
        {
            if (size == items.Length) EnsureCapacity(size + 1);
            items[size++] = item;
        }

        public void Clear()
        {
            if (0 < size)
            {
                Array.Clear(items, 0, size);
                size = 0;
            }
        }

        public bool Contains(T item)
        {
            return Contains(ref item);
        }

        public bool Contains(ref T item)
        {
            var c = EqualityComparer<T>.Default;
            for (int i = 0; i < size; i++)
            {
                if (c.Equals(items[i], item)) return true;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(items, 0, array, arrayIndex, size);
        }

        public bool Remove(T item)
        {
            return Remove(ref item);
        }

        public bool Remove(ref T item)
        {
            int index = IndexOf(item);
            if (0 <= index)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 末尾の要素を削除します。
        /// </summary>
        /// <remarks>
        /// 末尾以外の要素の削除では、
        /// 内部配列における削除位置を詰めるため、内部配列の部分的な複製が行われます。
        /// このため、可能ならば削除する要素を末尾とするロジックを組むことにより、
        /// 内部配列の部分的な複製を抑制することができます。
        /// 例えば、要素の順序が重要ではないならば、
        /// 削除する要素を末尾の要素と入れ替える事で、削除対象を末尾とすることができます。
        /// ただし、for 文などにより順序に基いて要素を参照している場合、
        /// 末尾との入れ替えはループの途中で順序の保証を失わせるため、
        /// 行うべきではありません。
        /// </remarks>
        public void RemoveLast()
        {
            RemoveAt(size - 1);
        }

        /// <summary>
        /// 要素をソートします。
        /// </summary>
        public void Sort()
        {
            Sort(0, Count, null);
        }

        /// <summary>
        /// 指定の比較オブジェクトを用いて要素をソートします。
        /// </summary>
        /// <param name="comparer">比較オブジェクト。</param>
        public void Sort(IComparer<T> comparer)
        {
            Sort(0, Count, comparer);
        }

        /// <summary>
        /// 指定の範囲にある要素をソートします。
        /// </summary>
        /// <param name="index">開始位置。</param>
        /// <param name="count">要素数。</param>
        /// <param name="comparer">比較オブジェクト。</param>
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            if (index < 0) throw new ArgumentOutOfRangeException("index");
            if (count < 0) throw new ArgumentOutOfRangeException("count");
            if (size - index < count) throw new ArgumentException("Invalid index and count.");

            Array.Sort<T>(items, index, count, comparer);
        }

        public void ForEach(Action<T> action)
        {
            if (action == null) throw new ArgumentNullException("action");

            for (int i = 0; i < size; i++)
            {
                action(items[i]);
            }
        }

        public void ForEach(RefAction<T> action)
        {
            if (action == null) throw new ArgumentNullException("action");

            for (int i = 0; i < size; i++)
            {
                action(ref items[i]);
            }
        }

        public void Swap(int index0, int index1)
        {
            T temp = items[index0];
            items[index0] = items[index1];
            items[index1] = temp;
        }

        /// <summary>
        /// 指定の位置にある要素と末尾の要素を入れ替えます。
        /// </summary>
        /// <param name="index">要素の位置。</param>
        public void SwapWithLast(int index)
        {
            Swap(index, size - 1);
        }

        void EnsureCapacity(int min)
        {
            // List<T> と同様の容量拡張ロジック。
            if (items.Length < min)
            {
                var newCapacity = items.Length == 0 ? defaultCapacity : items.Length * 2;
                if (newCapacity < min) newCapacity = min;
                Capacity = newCapacity;
            }
        }
    }
}
