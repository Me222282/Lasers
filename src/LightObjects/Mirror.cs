using Zene.Structs;

namespace Lasers
{
    public class Mirror : LightObject
    {
        public Mirror(Vector2 a, Vector2 b)
            : base(1)
        {
            _plain = new ReflectPlain(a, b);
            Segments[0] = _plain;
        }
        
        private ReflectPlain _plain;
        
        public Vector2 PointA
        {
            get => _plain.PointA;
            set => _plain.PointA = value;
        }
        public Vector2 PointB
        {
            get => _plain.PointB;
            set => _plain.PointB = value;
        }
        
        public override QueryData QueryMousePos(Vector2 mousePos, double range)
        {
            range *= range;
            
            if (mousePos.SquaredDistance(PointA) < range)
            {
                return new QueryData(0, PointA, this);
            }
            if (mousePos.SquaredDistance(PointB) < range)
            {
                return new QueryData(1, PointB, this);
            }
            
            return QueryData.Fail;
        }
        public override void MouseInteract(Vector2 mousePos, QueryData data)
        {
            base.MouseInteract(mousePos, data);
            
            if (data.PointNumber == 0)
            {
                PointA = mousePos;
                return;
            }
            
            PointB = mousePos;
        }
    }
}