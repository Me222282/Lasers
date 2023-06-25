using System;
using Zene.Structs;

namespace Lasers
{
    public struct RefractPlain : ILightInteractable
    {
        public RefractPlain(Vector2 a, Vector2 b, double m)
        {
            PointA = a;
            PointB = b;
            Medium = m;
        }
        
        public Vector2 PointA { get; set; }
        public Vector2 PointB { get; set; }
        public double Medium { get; set;}
        
        public void Render(LineDC context)
        {
            context.AddLine(new LineData(PointA, PointB, ColourF.DimGrey));
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
        
        private const double HalfPI = Math.PI / 2;
        
        public Ray InteractRay(Ray ray, Vector2 refPoint)
        {
            Vector2 diff = PointA - PointB;
            
            Radian lineA = Math.Atan2(diff.Y, diff.X);
            Radian dirA = Math.Atan2(ray.Line.Direction.Y, ray.Line.Direction.X);
            
            Radian i = dirA - (lineA + HalfPI);
            Radian r = Math.Asin((ray.Medium * Math.Sin(i)) / Medium);
            Radian newA = (HalfPI - r) - (lineA + r + r);
            
            return new Ray(refPoint, (Math.Cos(newA), Math.Sin(newA)), ray);
            //return new Ray(refPoint, (Math.Cos(newA), Math.Sin(newA)), Medium);
        }
    }
}