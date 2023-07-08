using System;
using System.Collections.Generic;
using Zene.Structs;

namespace Lasers
{
    public class RefractPlain : ILightInteractable
    {
        public RefractPlain(Vector2 a, Vector2 b, double m)
        {
            PointA = a;
            PointB = b;
            Medium = m;
        }
        
        public Vector2 PointA { get; set; }
        public Vector2 PointB { get; set; }
        public double Medium { get; set; }
        
        public ColourF3 Colour => ColourF3.LightGrey;
        public bool Curved => false;
        
        public void Render(LineDC context)
        {
            context.AddLine(new LineData(PointA, PointB, ColourF.LightGrey));
        }
        
        public Vector2 RayIntersection(Segment2 ray, bool lastIntersect)
        {
            if (lastIntersect)
            {
                return Vector2.PositiveInfinity;
            }
            
            Segment2 seg = new Segment2(PointA, PointB);
            
            if (ray.Intersects(seg, out Vector2 v))
            {
                return v;
            }
            
            return Vector2.PositiveInfinity;
        }
        
        private const double HalfPI = Math.PI / 2d;
        private const double TwoPI = Math.PI * 2d;
        
        public Ray InteractRay(Ray ray, Vector2 refPoint)
        {
            Vector2 diff = PointA - PointB;
            double m1 = ray.Medium;
            double m2 = Medium;
            List<double> mh = ray.MediumHistory;
            
            if (m1 == m2 && mh.Count > 1)
            {
                m1 = Medium;
                m2 = mh[^2];
            }
            else
            {
                mh.Add(m2);
            }
            
            Radian lineA = Math.Atan2(diff.Y, diff.X);
            if (lineA < 0)
            {
                lineA += Math.PI;
            }
            else if (lineA > Math.PI)
            {
                lineA -= Math.PI;
            }
            Radian dirA = Math.Atan2(ray.Line.Direction.Y, ray.Line.Direction.X);
            
            Radian i = dirA - (lineA + HalfPI);
            double sin = (m1 * Math.Sin(i)) / m2;
            
            // Total internal reflection
            if (sin > 1d || sin < -1d)
            {
                Radian reflect = (lineA * 2d) - dirA;
                
                return new Ray(refPoint, (Math.Cos(reflect), Math.Sin(reflect)), ray);
            }
            // Apply after potential total internal reflection
            if (m1 == Medium)
            {
                mh.RemoveAt(mh.Count - 1);
            }
            
            Radian r = Math.Asin(sin);
            Radian newA;
            double cosI = Math.Cos(i);
            if (cosI < 0)
            {
                newA = -r + lineA - HalfPI;
            }
            else
            {
                newA = r + lineA + HalfPI;
            }
            
            //return new Ray(refPoint, (Math.Cos(newA), Math.Sin(newA)), ray);
            return new Ray(refPoint, (Math.Cos(newA), Math.Sin(newA)), mh);
        }
    }
}