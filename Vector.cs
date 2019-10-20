namespace DragonLib
{
    public struct Vector<T>
    {
        public T X { get; set; }
        public T Y { get; set; }
        public T Z { get; set; }

        public (T x, T y, T z) AsTuple() => (X, Y, Z);
    }
}
