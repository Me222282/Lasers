using Zene.Structs;

namespace Lasers
{
    public class GlassPrism : TriObject
    {
        public GlassPrism(Vector2 a, Vector2 b, Vector2 c, double m)
            : base(new ColourF(0.7f, 0.7f, 0.7f, 0.7f))
        {
            AB = new RefractPlain(a, b, m);
            BC = new RefractPlain(b, c, m);
            CA = new RefractPlain(c, a, m);
            
            Segments[0] = AB;
            Segments[1] = BC;
            Segments[2] = CA;
            
            SetData();
        }
        
        private RefractPlain AB;
        private RefractPlain BC;
        private RefractPlain CA;
        
        protected override  Vector2 _pointA
        {
            get => AB.PointA;
            set
            {
                AB.PointA = value;
                CA.PointB = value;
            }
        }
        protected override  Vector2 _pointB
        {
            get => BC.PointA;
            set
            {
                BC.PointA = value;
                AB.PointB = value;
            }
        }
        protected override  Vector2 _pointC
        {
            get => CA.PointA;
            set
            {
                CA.PointA = value;
                BC.PointB = value;
            }
        }
        public double Medium
        {
            get => AB.Medium;
            set
            {
                AB.Medium = value;
                BC.Medium = value;
                CA.Medium = value;
            }
        }
    }
}