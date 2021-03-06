using JetBrains.Annotations;
using System;

namespace DragonLib
{
    [PublicAPI]
    public interface ISingleton<T> where T : class, new()
    {
        private static Lazy<T> SingletonInstance { get; } = new Lazy<T>(() => new T());

        public static T Instance => SingletonInstance.Value;
        public static bool IsCreated => SingletonInstance.IsValueCreated;
    }

    public class Singleton<T> : ISingleton<T> where T : class, new()
    {
        public static T Instance =>  ISingleton<T>.Instance;
        public static bool IsCreated =>  ISingleton<T>.IsCreated;
    }
}
