using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CS = System.Runtime.CompilerServices;

namespace DragonLib.Hash.Generic;

// modified https://github.com/lineplay/mt19937_64_cs/blob/master/mt19937_64.cs to take mt parameters.
public record struct MTRNGAlgorithm<T> where T : struct, IUnsignedNumber<T>, IBinaryInteger<T>, IMinMaxValue<T> {
    private readonly T A;
    private readonly T B;
    private readonly T C;
    private readonly T D;
    private readonly T Lower;
    private readonly ulong M;
    private readonly ulong N;
    private readonly T[] State;
    private readonly T Upper;
    private readonly int W;
    private readonly int X;
    private readonly int Y;
    private readonly int Z;
    private ulong Index;

    public MTRNGAlgorithm(T seed, ulong n, ulong m, int r, T a, T b, T c, T d, int w, int x, int y, int z, T f) {
        N = n;
        M = m;
        A = a;
        B = b;
        C = c;
        D = d;
        W = w;
        X = x;
        Y = y;
        Z = z;

        Lower = (T.One << r) - T.One;
        Upper = T.MaxValue - Lower;

        State = new T[N * 2];
        Index = N;
        var p = State[0] = T.CreateTruncating(seed);
        for (var i = T.One; i < T.CreateTruncating(N); ++i) {
            p = State[int.CreateTruncating(i)] = T.CreateTruncating(i + f * (p ^ (p >> (64 - 2))));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private void LORefill() {
        ulong i;
        for (i = 0; i < N - M; ++i) {
            var v = (State[i + N] & Upper) | (State[i + N + 1] & Lower);

            State[i] = (v >> 1) ^ Check(v) ^ State[i + N + M];
        }

        for (; i < N - 1; ++i) {
            var v = (State[i + N] & Upper) | (State[i + N + 1] & Lower);

            State[i] = (v >> 1) ^ Check(v) ^ State[i - N + M];
        }

        var vnext = (State[i + N] & Upper) | (State[0] & Lower);
        State[i] = (vnext >> 1) ^ Check(vnext) ^ State[M - 1];
        Index = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private void HIRefill() {
        for (var i = N; i < 2 * N; ++i) {
            var v = (State[i - N] & Upper) | (State[i - N + 1] & Lower);
            State[i] = (v >> 1) ^ Check(v) ^ State[i - N + M];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private T Check(T v) => (v & T.One) != T.Zero ? A : T.Zero;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public T Next() {
        if (Index == N) {
            HIRefill();
        } else if (Index >= 2 * N) {
            LORefill();
        }

        var r = State[Index++];
        r ^= (r >> W) & B;
        r ^= (r << X) & C;
        r ^= (r << Y) & D;
        r ^= r >> Z;
        return r;
    }

    public Span<byte> Bytes(int n = -1) {
        if (n == -1) {
            n = CS.Unsafe.SizeOf<T>();
        }

        Span<byte> bytes = new byte[n.Align(CS.Unsafe.SizeOf<T>())];
        var arr = MemoryMarshal.Cast<byte, T>(bytes);

        for (var i = 0; i < arr.Length; ++i) {
            arr[i] = Next();
        }

        return bytes[..n];
    }

    public void Bytes(Span<byte> bytes, int n = -1) {
        if (n == -1) {
            n = bytes.Length;
        }

        if ((n % CS.Unsafe.SizeOf<T>()) != 0) {
            Span<byte> tmp = stackalloc byte[n.Align(CS.Unsafe.SizeOf<T>())];
            var arr = MemoryMarshal.Cast<byte, T>(bytes);

            for (var i = 0; i < arr.Length; ++i) {
                arr[i] = Next();
            }

            tmp[..n].CopyTo(bytes);
        } else {
            var arr = MemoryMarshal.Cast<byte, T>(bytes);

            for (var i = 0; i < arr.Length; ++i) {
                arr[i] = Next();
            }
        }
    }
}
