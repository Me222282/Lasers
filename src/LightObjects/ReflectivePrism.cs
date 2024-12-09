using Zene.Structs;

namespace Lasers
{
    public class ReflectivePrism : TriObject
    {
        public ReflectivePrism(Vector2 a, Vector2 b, Vector2 c)
            : base(ColourF.White, false)
        {
            AB = new ReflectPlain(a, b);
            BC = new ReflectPlain(b, c);
            CA = new ReflectPlain(c, a);
            
            Segments[0] = AB;
            Segments[1] = BC;
            Segments[2] = CA;
        }
        
        private ReflectPlain AB;
        private ReflectPlain BC;
        private ReflectPlain CA;
        
        protected override Vector2 _pointA
        {
            get => AB.PointA;
            set
            {
                AB.PointA = value;
                CA.PointB = value;
            }
        }
        protected override Vector2 _pointB
        {
            get => BC.PointA;
            set
            {
                BC.PointA = value;
                AB.PointB = value;
            }
        }
        protected override Vector2 _pointC
        {
            get => CA.PointA;
            set
            {
                CA.PointA = value;
                BC.PointB = value;
            }
        }
    }
}