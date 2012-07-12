#region Using

using System;
using System.Collections.Generic;

#endregion

namespace TerrainDemo.CDLOD
{
    public sealed class NodeCollection : ICollection<Node>
    {
        Node[] nodes = new Node[4];

        int count;

        public void Add(Node item)
        {
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(Node item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Node[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(Node item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Node> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
