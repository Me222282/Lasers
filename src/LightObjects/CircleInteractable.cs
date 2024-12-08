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
            context.DrawRing(new Box(Location, Radius * 2d), 1d * context.Multiplier, (ColourF)Colour, InnerColour);
        }
        
        private const double _tolerance = 0.00001;
        
        public Vector2 RayIntersection(Segment2 ray, bool lastIntersect)
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
                
                if (t1 < 0d)
                {
                    t = t2;
                    T = t1;
                }
                else if (t2 < 0d)
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
            
            // No rereflect issues
            if (!lastIntersect)
            {
                return ray.A + (t * change);
            }
            
            // Heading away from circle centre - detection is a rereflect
            if (offset.SquaredLength < Location.SquaredDistance(ray.A + (change * _tolerance * Radius / change.SquaredLength)))
            {
                return Vector2.PositiveInfinity;
            }
            
            if (T <= 0d)
            {
                return ray.A + (t * change);
            }
            
            // Use other intersection
            return ray.A + (T * change);
        }
        
        public abstract Ray InteractRay(Ray ray, Vector2 refPoint);
    }
}