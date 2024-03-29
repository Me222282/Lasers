using Zene.Structs;

namespace Lasers
{
    public class Mirror : LineObject
    {
        public Mirror(Vector2 a, Vector2 b)
            : base(1)
        {
            _plain = new ReflectPlain(a, b);
            Segments[0] = _plain;
        }
        
        private ReflectPlain _plain;
        
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