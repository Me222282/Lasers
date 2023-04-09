using System;
using Zene.Structs;

namespace Lasers
{
    public struct ReflectPlain : ILightInteractable
    {
        public ReflectPlain(Vector2 a, Vector2 b)
        {
            PointA = a;
            PointB = b;
        }
        
        public Vector2 PointA { get; set; }
        public Vector2 PointB { get; set; }
        
        public void Render(LineDC context)
        {
            context.DrawLine(new LineData(PointA, PointB, ColourF.White));
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
            Vector2 diff = PointA - PointB;
            
            Radian lineA = Math.Atan2(diff.Y, diff.X);
            Radian dirA = Math.Atan2(ray.Line.Direction.Y, ray.Line.Direction.X);
            
            Radian newA = (lineA * 2d) - dirA;
            
            return new Ray(refPoint, (Math.Cos(newA), Math.Sin(newA)), ray);
        }
    }
}