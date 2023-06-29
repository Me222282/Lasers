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
        
        public virtual void Render(LineDC context)
        {
            context.DrawRing(new Box(Location, Radius * 2d), 1d * context.Multiplier, (ColourF)Colour);
        }
        
        public Vector2 RayIntersection(Segment2 ray)
        {
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
            
            if (discriminant == 0)
            {
                t = (-b / (2d * a));
            }
            else
            {
                discriminant = Math.Sqrt(discriminant);
                
                double t1 = ((discriminant - b) / (2d * a));
                double t2 = ((-discriminant - b) / (2d * a));
                
                if (t1 < 0d)
                {
                    t = t2;
                }
                else if (t2 < 0d)
                {
                    t = t1;
                }
                else
                {
                    t = t1 < t2 ? t1 : t2;
                }
            }
            
            if (t > 1d || t < 0d)
            {
                return Vector2.PositiveInfinity;
            }
            
            return ray.A + (t * change);
        }
        
        public abstract Ray InteractRay(Ray ray, Vector2 refPoint);
    }
}