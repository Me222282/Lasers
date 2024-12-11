using Zene.Structs;

namespace Lasers
{
    public class MirrorArc : LineObject
    {
        public MirrorArc(Vector2 a, Vector2 b, double c)
            : base(1)
        {
            _arc = new ReflectArc(a, b, c);
            Segments[0] = _arc;
        }
        
        private ReflectArc _arc;
        
        public override Vector2 PointA
        {
            get => _arc.PointA;
            set => _arc.PointA = value;
        }
        public override Vector2 PointB
        {
            get => _arc.PointB;
            set => _arc.PointB = value;
        }
        public double Curve
        {
            get => _arc.Curve;
            set => _arc.Curve = value;
        }
    }
}