using System;
using Zene.Structs;
using Zene.Graphics;

namespace Lasers
{
    public class ReflectArc : ArcInteractable
    {
        public ReflectArc(Vector2 a, Vector2 b, double c)
            : base(a, b, c)
        {
            
        }
        
        public override Ray InteractRay(LightingEngine engine, Ray ray, Vector2 refPoint)
        {
            Vector2 diff = _centre - refPoint;
            Line2 reflect = new Line2(diff, refPoint);
            Vector2 np = reflect.Reflect(ray.Line.Location);
            
            return new Ray(refPoint, (np - refPoint).Normalised(), ray.Medium);
        }
    }
}