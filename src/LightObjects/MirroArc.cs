using System;
using Zene.Structs;

namespace Lasers
{
    public class MirrorArc : LightObject
    {
        public MirrorArc(Vector2 a, Vector2 b, double c)
            : base(1)
        {
            _arc = new ReflectArc(a, b, c);
            Segments[0] = _arc;
        }
        
        private ReflectArc _arc;
        
        public Vector2 PointA
        {
            get => _arc.PointA;
            set => _arc.PointA = value;
        }
        public Vector2 PointB
        {
            get => _arc.PointB;
            set => _arc.PointB = value;
        }
        public double Curve
        {
            get => _arc.Curve;
            set => _arc.Curve = value;
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
            if (mousePos.SquaredDistance(_arc.CP) < range)
            {
                return new QueryData(2, _arc.CP, this);
            }
            
            return QueryData.Fail;
        }
        public override Vector2 MouseInteract(Vector2 mousePos, QueryData data)
        {
            base.MouseInteract(mousePos, data);
            
            if (data.PointNumber == 0)
            {
                PointA = mousePos;
                return mousePos;
            }
            if (data.PointNumber == 1)
            {
                PointB = mousePos;
                return mousePos;
            }
            
            Vector2 a = PointA;
            Vector2 b = PointB;
            
            Vector2 t = b - a;
            
            Line2 l1 = new Line2(t, mousePos);
            t.Rotate90();
            Line2 l2 = new Line2(t, data.Location);
            Vector2 cpn = l1.Intersects(l2);
            
            Vector2 mid = (a + b) / 2d;
            Vector2 c = (cpn - mid) / t;
            
            Curve = c.X;
            
            return cpn;
        }
        
        protected override void AddOffset(Vector2 offset)
        {
            PointA += offset;
            PointB += offset;
        }
        protected internal override bool IsMouseOverObject(Vector2 mousePos, double range)
        {
            if (!_arc.InSector(mousePos)) { return false; }
            
            double dist = mousePos.SquaredDistance(_arc.Centre);
            double min = _arc.Rad - range;
            double max = _arc.Rad + range;
            
            return (min * min) <= dist && dist <= (max * max);
        }
    }
}