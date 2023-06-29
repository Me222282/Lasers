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
        
        public override Ray InteractRay(Ray ray, Vector2 refPoint)
        {
            Vector2 diff = Location - refPoint;
            
            Radian lineA = Math.Atan2(-diff.X, diff.Y);
            Radian dirA = Math.Atan2(ray.Line.Direction.Y, ray.Line.Direction.X);
            
            Radian newA = (lineA * 2d) - dirA;
            
            return new Ray(refPoint, (Math.Cos(newA), Math.Sin(newA)), ray);
        }
    }
}