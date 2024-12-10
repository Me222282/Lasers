using System;
using System.Collections.Generic;
using Zene.Structs;

namespace Lasers
{
    public class RefractPlain : ILightInteractable
    {
        public RefractPlain(Vector2 a, Vector2 b, double m, LightObject obj)
        {
            PointA = a;
            PointB = b;
            Medium = m;
            
            _obj = obj;
        }
        
        private LightObject _obj;
        
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
        
        public Ray InteractRay(LightingEngine engine, Ray ray, Vector2 refPoint)
        {
            Vector2 diff = PointA - PointB;
            
            Radian lineA = Math.Atan2(diff.Y, diff.X);
            if (lineA < 0)
            {
                lineA += Math.PI;
            }
            else if (lineA > Math.PI)
            {
                lineA -= Math.PI;
            }
            double m = Medium;
            if (ray.Medium == m)
            {
                m = engine.GetMedium(refPoint, _obj);
            }
            
            return Extensions.Refract(m, ray, refPoint, lineA);
        }
    }
}