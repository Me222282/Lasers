using System;
using Zene.Structs;

namespace Lasers
{
    public class ReflectCircle : CircleInteractable
    {
        public ReflectCircle(Vector2 pos, double radius)
            : base(pos, radius, ColourF3.White)
        {
            
        }
        
        public override Ray InteractRay(ResolveRayArgs args)
        {
            Vector2 refPoint = args.Point;
            Ray ray = args.Ray;
            
            Vector2 diff = Location - refPoint;
            Line2 reflect = new Line2(diff, refPoint);
            Vector2 np = reflect.Reflect(ray.Line.Location);
            
            return new Ray(refPoint, (np - refPoint).Normalised(), ray.Medium);
        }
    }
}