#region Using

using System;
using System.Collections.Generic;

#endregion

namespace TiledTerrainDemo.Landscape
{
    public sealed class Pool<T> where T : class
    {
        public const int DefaultInitialCapacity = 10;

        Func<T> creationFunction;

        Stack<T> objects = new Stack<T>();

        public Pool(Func<T> creationFunction)
            : this(creationFunction, DefaultInitialCapacity)
        {
        }

        public Pool(Func<T> creationFunction, int initialCapacity)
        {
            this.creationFunction = creationFunction;

            for (int i = 0; i < initialCapacity; i++)
                objects.Push(creationFunction());
        }

        public T Borrow()
        {
            if (objects.Count != 0)
                return objects.Pop();

            return creationFunction();
        }

        public void Return(T partition)
        {
            objects.Push(partition);
        }
    }
}
