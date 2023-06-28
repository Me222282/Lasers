using Zene.Structs;

namespace Lasers
{
    public class Mat : LineObject
    {
        public Mat(Vector2 a, Vector2 b)
            : base(1)
        {
            _plain = new AbsorbPlain(a, b);
            Segments[0] = _plain;
        }
        
        private AbsorbPlain _plain;
        
        public override Vector2 PointA
        {
            get => _plain.PointA;
            set => _plain.PointA = value;
        }
        public override Vector2 PointB
        {
            get => _plain.PointB;
            set => _plain.PointB = value;
        }
    }
}