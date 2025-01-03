using System;
using System.Collections.Generic;
using Zene.Graphics;
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
        
        public void OnRender(IDrawingContext context, DrawArgs args)
        {
            args.Lines.Add(new LineData(PointA, PointB, ColourF.LightGrey));
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
        
        private const double HalfPI = Math.PI / 2d;
        private const double TwoPI = Math.PI * 2d;
        
        public Ray InteractRay(ResolveRayArgs args)
        {
            Vector2 refPoint = args.Point;
            Ray ray = args.Ray;
            
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
                m = args.Engine.GetMedium(refPoint, args.Source);
            }
            
            return Extensions.Refract(m, ray, refPoint, lineA);
        }
    }
}