#region Using

using System;
using System.Collections.Generic;

#endregion

namespace LiSPSMDemo
{
    /// <summary>
    /// オブジェクトのプーリングを管理するクラスです。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Pool<T> : IEnumerable<T> where T : class
    {
        // 0 は容量制限無し。
        public const int DefaultMaxCapacity = 0;

        Func<T> createFunction;

        Queue<T> objects = new Queue<T>();

        public int Count
        {
            get { return objects.Count; }
        }

        public int InitialCapacity { get; private set; }

        public int MaxCapacity { get; set; }

        public int TotalObjectCount { get; private set; }

        /// <summary>
        /// オブジェクトの生成関数を指定してインスタンス生成します。
        /// オブジェクトの生成関数は、プールで新たなインスタンスの生成が必要となった場合に呼び出されます。
        /// </summary>
        /// <param name="createFunction">オブジェクトの生成関数。</param>
        public Pool(Func<T> createFunction)
        {
            if (createFunction == null) throw new ArgumentNullException("createFunction");

            this.createFunction = createFunction;

            MaxCapacity = DefaultMaxCapacity;
        }

        /// <summary>
        /// プールからオブジェクトを取得します。
        /// プールが空の場合、オブジェクトを新たに生成して返します。
        /// ただし、MaxCapacity に 0 以上を指定し、かつ、
        /// プールから生成したオブジェクトの総数が上限を越える場合には、null を返します。
        /// </summary>
        /// <returns></returns>
        public T Borrow()
        {
            while (0 < MaxCapacity && MaxCapacity < TotalObjectCount && 0 < objects.Count)
                DisposeObject(objects.Dequeue());

            if (0 < MaxCapacity && MaxCapacity <= TotalObjectCount && objects.Count == 0)
                return null;

            if (0 < objects.Count)
                return objects.Dequeue();

            return CreateObject();
        }

        /// <summary>
        /// オブジェクトをプールへ戻します。
        /// </summary>
        /// <param name="obj"></param>
        public void Return(T obj)
        {
            if (MaxCapacity == 0 || TotalObjectCount <= MaxCapacity)
            {
                objects.Enqueue(obj);
            }
            else
            {
                DisposeObject(obj);
            }
        }

        /// <summary>
        /// プール内の全てのオブジェクトを破棄します。
        /// </summary>
        public void Clear()
        {
            foreach (var obj in objects) DisposeObject(obj);

            objects.Clear();
        }

        T CreateObject()
        {
            if (0 < MaxCapacity && MaxCapacity < TotalObjectCount)
                return null;

            TotalObjectCount++;
            return createFunction();
        }

        void DisposeObject(T obj)
        {
            var disposable = obj as IDisposable;
            if (disposable != null) disposable.Dispose();

            TotalObjectCount--;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return objects.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return objects.GetEnumerator();
        }
    }
}
