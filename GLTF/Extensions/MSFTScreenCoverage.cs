using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace DragonLib.GLTF.Extensions
{
    [PublicAPI]
    public class MSFTScreenCoverage : GLTFProperty, IGLTFExtension, IList<int>
    {
        public const string Identifier = "MSFT_screencoverage";
        public List<int> BackingList { get; set; } = new List<int>();

        public void Insert(GLTFProperty gltf, GLTFRoot root)
        {
            if (!(gltf is GLTFNode node)) return;
            node.Extras[Identifier] = this;
            root.ExtensionsUsed.Add(Identifier);
        }

        public IEnumerator<int> GetEnumerator() => BackingList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) BackingList).GetEnumerator();

        public void Add(int item) => BackingList.Add(item);

        public void Clear() => BackingList.Clear();

        public bool Contains(int item) => BackingList.Contains(item);

        public void CopyTo(int[] array, int arrayIndex) => BackingList.CopyTo(array, arrayIndex);

        public bool Remove(int item) => BackingList.Remove(item);

        public int Count => BackingList.Count;

        public bool IsReadOnly => ((ICollection<int>) BackingList).IsReadOnly;

        public int IndexOf(int item) => BackingList.IndexOf(item);

        public void Insert(int index, int item) => BackingList.Insert(index, item);

        public void RemoveAt(int index) => BackingList.RemoveAt(index);

        public int this[int index]
        {
            get => BackingList[index];
            set => BackingList[index] = value;
        }
    }
}
