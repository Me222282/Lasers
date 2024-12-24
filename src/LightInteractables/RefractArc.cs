using System;
using Zene.Structs;
using Zene.Graphics;

namespace Lasers
{
    public class RefractArc : ArcInteractable
    {
        public RefractArc(Vector2 a, Vector2 b, double c, double m, LightObject obj)
            : base(a, b, c)
        {
            Medium = m;
            _obj = obj;
        }
        
        private LightObject _obj;
        
        public double Medium { get; set; }
        
        public override Ray InteractRay(LightingEngine engine, Ray ray, Vector2 refPoint)
        {
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
                m = engine.GetMedium(refPoint, _obj);
            }
            
            return Extensions.Refract(m, ray, refPoint, lineA);
        }
    }
}