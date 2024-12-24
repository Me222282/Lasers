using System;
using Zene.Graphics;
using Zene.Structs;

namespace Lasers
{
    public abstract class CircleInteractable : ILightInteractable
    {
        public CircleInteractable(Vector2 pos, double radius, ColourF3 c)
        {
            Location = pos;
            Radius = radius;
            Colour = c;
        }
        
        public Vector2 Location { get; set; }
        public double Radius { get; set; }
        
        public ColourF3 Colour { get; set; }
        public ColourF InnerColour { get; set; }
        
        public virtual void Render(LineDC context)
        {
            context.DrawBorderEllipse(new Box(Location, Radius * 2d), 1d * context.Multiplier, InnerColour, (ColourF)Colour);
        }
        
        private const double _tolerance = 0.00001;
        
        public Vector2 RayIntersection(FindRayArgs args)
        {
            Segment2 ray = args.Ray;
            
            Vector2 change = ray.Change;
            Vector2 offset = ray.A - Location;
            
            double a = change.Dot(change);
            double b = 2d * offset.Dot(change);
            double c = offset.Dot(offset) - (Radius * Radius);
            
            double discriminant = (b * b) - (4 * a * c);
            // No intersection
            if (discriminant < 0)
            {
                return Vector2.PositiveInfinity;
            }
            
            double t;
            double T = 0d;
            
            if (discriminant == 0)
            {
                t = (-b / (2d * a));
            }
            else
            {
                discriminant = Math.Sqrt(discriminant);
                
                double t1 = ((discriminant - b) / (2d * a));
                double t2 = ((-discriminant - b) / (2d * a));
                
                if (t1 <= 0d)
                {
                    t = t2;
                    T = t1;
                }
                else if (t2 <= 0d)
                {
                    t = t1;
                    T = t2;
                }
                else
                {
                    t = Math.Min(t1, t2);
                    T = Math.Max(t1, t2);
                }
            }
            
            if (t > 1d || t < 0d)
            {
                return Vector2.PositiveInfinity;
            }
            
            Vector2 p = ray.A + (t * change);
            
            // No rereflect issues
            if (!args.LastIntersect)
            {
                return p;
            }
            
            bool inside = (ray.A - Location).Dot(ray.Change) < 0d;
            
            // heading away or there are 2 points
            if (!inside || T > 0d)
            {
                t = -1d;
            }
            
            // Use other intersection
            if (t <= 0d)
            {
                if (T <= 0d)
                {
                    return Vector2.PositiveInfinity;
                }
                
                return ray.A + (T * change);
            }
            
            return p;
        }
        
        public abstract Ray InteractRay(ResolveRayArgs args);
    }
}