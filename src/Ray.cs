using System.Collections.Generic;
using Zene.Structs;

namespace Lasers
{
    public struct Ray
    {
        public Ray(Line2 line, List<double> mh)
        {
            Line = line;
            MediumHistory = mh;
        }
        public Ray(Line2 line, Ray source)
        {   
            Line = line;
            MediumHistory = source.MediumHistory;
        }
        public Ray(Vector2 point, Vector2 dir, List<double> mh)
        {   
            Line = new Line2(dir, point);
            MediumHistory = mh;
        }
        public Ray(Vector2 point, Vector2 dir, Ray source)
        {   
            Line = new Line2(dir, point);
            MediumHistory = source.MediumHistory;
        }
        
        public Line2 Line { get; }
        public double Medium => MediumHistory[^1];
        public List<double> MediumHistory { get; }
    }
}