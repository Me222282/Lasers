using System;
using Zene.Structs;

namespace Lasers
{
    public class ReflectPlain : ILightInteractable
    {
        public ReflectPlain(Vector2 a, Vector2 b)
        {
            PointA = a;
            PointB = b;
        }
        
        public Vector2 PointA { get; set; }
        public Vector2 PointB { get; set; }
        
        public ColourF3 Colour => ColourF3.White;
        
        public void Render(LineDC context)
        {
            context.AddLine(new LineData(PointA, PointB, ColourF.White));
        }
        
        public Vector2 RayIntersection(FindRayArgs args)
        {
            if (args.LastIntersect)
            {
                return Vector2.PositiveInfinity;
            }
            
            Segment2 seg = new Segment2(PointA, PointB);
            
            if (args.Ray.Intersects(seg, out Vector2 v))
            {
                return v;
            }
            
            return Vector2.PositiveInfinity;
        }
        
        public Ray InteractRay(ResolveRayArgs args)
        {
            Vector2 refPoint = args.Point;
            Ray ray = args.Ray;
            
            Vector2 diff = PointA - PointB;
            Line2 reflect = new Line2(diff.Rotated90(), refPoint);
            Vector2 np = reflect.Reflect(ray.Line.Location);
            
            return new Ray(refPoint, (np - refPoint).Normalised(), ray.Medium);
        }
    }
}