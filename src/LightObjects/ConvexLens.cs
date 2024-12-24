using System;
using Zene.Structs;

namespace Lasers
{
    public class ConvexLens : LightObject
    {
        public ConvexLens(Vector2 a, Vector2 b, double c, double m)
            : base(2)
        {
            _arc1 = new RefractArc(a, b, c, m, this);
            _arc2 = new RefractArc(b, a, c, m, this);
            Segments[0] = _arc1;
            Segments[1] = _arc2;
            
            _arc1.InnerColour = new ColourF(0.7f, 0.7f, 0.7f, 0.7f);
            _arc2.InnerColour = new ColourF(0.7f, 0.7f, 0.7f, 0.7f);
        }
        
        private RefractArc _arc1;
        private RefractArc _arc2;
        
        public Vector2 PointA
        {
            get => _arc1.PointA;
            set
            {
                _arc1.PointA = value;
                _arc2.PointB = value;
            }
        }
        public Vector2 PointB
        {
            get => _arc1.PointB;
            set
            {
                _arc1.PointB = value;
                _arc2.PointA = value;
            }
        }
        public double Curve
        {
            get => _arc1.Curve;
            set
            {
                _arc1.Curve = value;
                _arc2.Curve = value;
            }
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
            if (mousePos.SquaredDistance(_arc1.CP) < range)
            {
                return new QueryData(2, _arc1.CP, this);
            }
            if (mousePos.SquaredDistance(_arc2.CP) < range)
            {
                return new QueryData(3, _arc2.CP, this);
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
            
            if (data.PointNumber == 2)
            {
                Curve = c.X;
            }
            else if (data.PointNumber == 3)
            {
                Curve = -c.X;
            }
            
            return cpn;
        }
        
        protected override void AddOffset(Vector2 offset)
        {
            PointA += offset;
            PointB += offset;
        }
        protected internal override bool IsMouseOverObject(Vector2 mousePos, double range)
        {
            double dist1 = mousePos.SquaredDistance(_arc1.Centre);
            double dist2 = mousePos.SquaredDistance(_arc2.Centre);
            double r2 = _arc1.Rad * _arc1.Rad;
            
            if (_arc1.Curve > 0.5)
            {
                return dist1 <= r2 || dist2 <= r2;
            }
            
            return dist1 <= r2 && dist2 <= r2;
        }
    }
}