using System;
using Zene.Structs;

namespace Lasers
{
    public class ReflectiveBlock : QuadObject
    {
        public ReflectiveBlock(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
            : base(4, ColourF.White, false)
        {
            AB = new ReflectPlain(a, b);
            BC = new ReflectPlain(b, c);
            CD = new ReflectPlain(c, d);
            DA = new ReflectPlain(d, a);
            
            Segments[0] = AB;
            Segments[1] = BC;
            Segments[2] = CD;
            Segments[3] = DA;
        }
        
        private ReflectPlain AB;
        private ReflectPlain BC;
        private ReflectPlain CD;
        private ReflectPlain DA;
        
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
    }
}