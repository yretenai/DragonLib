namespace DragonLib
{
    public struct Quaternion<T>
    {
        public T X { get; set; }
        public T Y { get; set; }
        public T Z { get; set; }
        public T W { get; set; }

        public (T x, T y, T z, T w) AsTuple() => (X, Y, Z, W);
    }
}
