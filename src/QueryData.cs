using System;
using Zene.Structs;

namespace Lasers
{
    public struct QueryData
    {
        public QueryData(int p, Vector2 l, IPointHover source)
        {
            PointNumber = p;
            Location = l;
            Source = source;
            Scroll = 0d;
            Shift = false;
            Control = false;
        }
        
        public int PointNumber { get; }
        public Vector2 Location { get; }
        public IPointHover Source { get; }
        public double Scroll { get; set; }
        
        public bool Shift { get; set; }
        public bool Control { get; set; }
        
        public bool Pass() => Source != null;

        public override bool Equals(object obj)
        {
            return obj is QueryData data &&
                   Source == data.Source &&
                   PointNumber == data.PointNumber;
        }
        public override int GetHashCode() => HashCode.Combine(PointNumber, Location, Source);

        public static QueryData Fail { get; } = new QueryData();
        
        public static bool operator ==(QueryData a, QueryData b) =>  a.Equals(b);
        public static bool operator !=(QueryData a, QueryData b) =>  !a.Equals(b);
    }
}