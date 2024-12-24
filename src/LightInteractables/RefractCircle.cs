using System;
using Zene.Structs;

namespace Lasers
{
    public class RefractCircle : CircleInteractable
    {
        public RefractCircle(Vector2 pos, double radius, double m)
            : base(pos, radius, ColourF3.White)
        {
            Medium = m;
        }
        
        public double Medium { get; set; }
        
        public override Ray InteractRay(ResolveRayArgs args)
        {
            Vector2 refPoint = args.Point;
            Ray ray = args.Ray;
            
            Vector2 diff = refPoint - Location;
            diff.Rotate90();
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