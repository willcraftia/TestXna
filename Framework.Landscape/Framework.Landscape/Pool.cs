#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Willcraftia.Xna.Framework.Landscape
{
    public sealed class Pool<T> where T : class
    {
        public const int DefaultInitialCapacity = 10;

        public const int DefaultMaxCapacity = 0;

        Func<T> createFunction;

        Stack<T> objects = new Stack<T>();

        public int InitialCapacity { get; private set; }

        public int MaxCapacity { get; private set; }

        public int TotalObjectCount { get; private set; }

        public int FreeObjectCount
        {
            get { return objects.Count; }
        }

        public Pool(Func<T> createFunction)
            : this(createFunction, DefaultInitialCapacity, DefaultMaxCapacity)
        {
        }

        public Pool(Func<T> createFunction, int initialCapacity, int maxCapacity)
        {
            if (0 < maxCapacity && maxCapacity < initialCapacity)
                throw new ArgumentOutOfRangeException("initialCapacity");

            this.createFunction = createFunction;
            InitialCapacity = initialCapacity;
            MaxCapacity = maxCapacity;

            for (int i = 0; i < initialCapacity; i++)
                objects.Push(CreateObject());
        }

        public T Borrow()
        {
            if (objects.Count != 0)
                return objects.Pop();

            return CreateObject();
        }

        public void Return(T partition)
        {
            objects.Push(partition);
        }

        T CreateObject()
        {
            if (0 < MaxCapacity && TotalObjectCount == MaxCapacity)
                return null;

            TotalObjectCount++;
            return createFunction();
        }
    }
}
