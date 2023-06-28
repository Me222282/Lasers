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
        
        public ColourF3 Colour => ColourF3.LightGreen;
        
        public void Render(LineDC context)
        {
            context.AddLine(new LineData(PointA, PointB, ColourF.LightGreen));
        }
        
        public Vector2 RayIntersection(Segment2 ray)
        {
            Segment2 seg = new Segment2(PointA, PointB);
            
            if (ray.Intersects(seg, out Vector2 v))
            {
                return v;
            }
            
            return Vector2.PositiveInfinity;
        }
        
        public Ray InteractRay(Ray ray, Vector2 refPoint)
        {
            return new Ray(refPoint, Vector2.Zero, ray.MediumHistory);
        }
    }
}