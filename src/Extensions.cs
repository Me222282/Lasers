using System;
using System.IO;
using System.Runtime.InteropServices;
using Zene.Structs;

namespace Lasers
{
    internal static class Extensions
    {
        public static unsafe void Write<T>(this Stream stream, T value) where T : unmanaged
            => stream.Write(new ReadOnlySpan<byte>(&value, sizeof(T)));

        public static unsafe void Write<T>(this Stream stream, T[] values) where T : unmanaged
        {   
            stream.Write(values.Length);
            
            if (values.Length == 0) { return; }
            
            fixed (T* ptr = &values[0])
            {
                stream.Write(new ReadOnlySpan<byte>(ptr, values.Length * sizeof(T)));
            }
        }

        public static unsafe T Read<T>(this Stream stream) where T : unmanaged
        {
            Span<byte> data = stackalloc byte[sizeof(T)];
            int r = stream.Read(data);
            
            // End of stream
            if (r == 0) { return default; }
            
            return MemoryMarshal.Cast<byte, T>(data)[0];
        }

        public static unsafe T[] ReadArray<T>(this Stream stream) where T : unmanaged
        {
            int length = stream.Read<int>();
            
            // Array data incorrect
            if ((stream.Length - stream.Position) < length)
            {
                return null;
            }
            
            T[] array = new T[length];

            for (int i = 0; i < length; i++)
            {
                array[i] = stream.Read<T>();
            }

            return array;
        }
        
        public static double Distance(this Vector2 p, Segment2 s)
        {
            double n = Math.Abs(((s.B.X - s.A.X) * (s.A.Y - p.Y)) - ((s.A.X - p.X) * (s.B.Y - s.A.Y)));
            double d = s.A.Distance(s.B);
            
            return n / d;
        }
        public static double SquaredDistance(this Vector2 p, Segment2 s)
        {
            double n = Math.Abs(((s.B.X - s.A.X) * (s.A.Y - p.Y)) - ((s.A.X - p.X) * (s.B.Y - s.A.Y)));
            double d = s.A.SquaredDistance(s.B);
            
            return (n * n) / d;
        }
    }
}