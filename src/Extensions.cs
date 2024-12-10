using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Zene.Structs;

namespace Lasers
{
    internal static class Extensions
    {
        private const double HalfPI = Math.PI / 2d;
        private const double TwoPI = Math.PI * 2d;
        
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
        
        public static Ray Refract(double m2, Ray ray, Vector2 refPoint, Radian lineA)
        {
            double m1 = ray.Medium;
            
            Radian dirA = Math.Atan2(ray.Line.Direction.Y, ray.Line.Direction.X);
            
            Radian i = dirA - (lineA + HalfPI);
            double sin = (m1 * Math.Sin(i)) / m2;
            
            // Total internal reflection
            if (sin > 1d || sin < -1d)
            {
                Radian reflect = (lineA * 2d) - dirA;
                
                return new Ray(refPoint, (Math.Cos(reflect), Math.Sin(reflect)), m2);
            }
            
            Radian r = Math.Asin(sin);
            Radian newA;
            double cosI = Math.Cos(i);
            if (cosI < 0)
            {
                newA = -r + lineA - HalfPI;
            }
            else
            {
                newA = r + lineA + HalfPI;
            }
            
            //return new Ray(refPoint, (Math.Cos(newA), Math.Sin(newA)), ray);
            return new Ray(refPoint, (Math.Cos(newA), Math.Sin(newA)), m2);
        }
    }
}