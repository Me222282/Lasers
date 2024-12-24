using System;
using Zene.Structs;

namespace Lasers
{
    public class GlassBlock : QuadObject
    {
        public GlassBlock(Vector2 a, Vector2 b, Vector2 c, Vector2 d, double m)
            : base(new ColourF(0.7f, 0.7f, 0.7f, 0.7f))
        {
            AB = new RefractPlain(a, b, m);
            BC = new RefractPlain(b, c, m);
            CD = new RefractPlain(c, d, m);
            DA = new RefractPlain(d, a, m);
            
            Segments[0] = AB;
            Segments[1] = BC;
            Segments[2] = CD;
            Segments[3] = DA;
            
            SetData();
        }
        
        private RefractPlain AB;
        private RefractPlain BC;
        private RefractPlain CD;
        private RefractPlain DA;
        
        protected override  Vector2 _pointA
        {
            get => AB.PointA;
            set
            {
                AB.PointA = value;
                DA.PointB = value;
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
            get => CD.PointA;
            set
            {
                CD.PointA = value;
                BC.PointB = value;
            }
        }
        protected override Vector2 _pointD
        {
            get => DA.PointA;
            set
            {
                DA.PointA = value;
                CD.PointB = value;
            }
        }
        public override double Medium
        {
            get => AB.Medium;
            set
            {
                AB.Medium = value;
                BC.Medium = value;
                CD.Medium = value;
                DA.Medium = value;
            }
        }
    }
}