using System;
using Zene.Structs;

namespace Lasers
{
    public class AbsorbPlain : ILightInteractable
    {
        public AbsorbPlain(Vector2 a, Vector2 b)
        {
            PointA = a;
            PointB = b;
        }
        
        public Vector2 PointA { get; set; }
        public Vector2 PointB { get; set; }
        
        public ColourF3 Colour => ColourF3.IndianRed;
        
        public void Render(LineDC context)
        {
            context.AddLine(new LineData(PointA, PointB, ColourF.IndianRed));
        }
        
        public Vector2 RayIntersection(FindRayArgs args)
        {
            Segment2 seg = new Segment2(PointA, PointB);
            
            if (args.Ray.Intersects(seg, out Vector2 v))
            {
                return v;
            }
            
            return Vector2.PositiveInfinity;
        }
        
        public Ray InteractRay(ResolveRayArgs args)
        {
            return new Ray(args.Point, Vector2.Zero, args.Ray.Medium);
        }
    }
}