using Zene.Structs;

namespace Lasers
{
    public class RefractGlass : LightObject
    {
        public RefractGlass(Vector2 a, Vector2 b, double m)
            : base(1)
        {
            Segments[0] = new RefractPlain(a, b, m);
        }
        
        public Vector2 PointA
        {
            get => ((RefractPlain)Segments[0]).PointA;
            set
            {
                RefractPlain rp = (RefractPlain)Segments[0];
                Segments[0] = new RefractPlain(value, rp.PointB, rp.Medium);
            }
        }
        public Vector2 PointB
        {
            get => ((RefractPlain)Segments[0]).PointB;
            set
            {
                RefractPlain rp = (RefractPlain)Segments[0];
                Segments[0] = new RefractPlain(rp.PointA, value, rp.Medium);
            }
        }
        public double Medium
        {
            get => ((RefractPlain)Segments[0]).Medium;
            set
            {
                RefractPlain rp = (RefractPlain)Segments[0];
                Segments[0] = new RefractPlain(rp.PointA, rp.PointB, value);
            }
        }
    }
}