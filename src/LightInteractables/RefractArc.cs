using System;
using Zene.Structs;
using Zene.Graphics;

namespace Lasers
{
    public class RefractArc : ArcInteractable
    {
        public RefractArc(Vector2 a, Vector2 b, double c, double m)
            : base(a, b, c)
        {
            Medium = m;
        }
        
        public double Medium { get; set; }
        
        public override Ray InteractRay(ResolveRayArgs args)
        {
            Vector2 refPoint = args.Point;
            Ray ray = args.Ray;
            
            Vector2 diff = refPoint - _centre;
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