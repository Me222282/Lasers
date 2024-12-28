using System.Collections.Generic;
using Zene.Structs;

namespace Lasers
{
    public struct Ray
    {
        public Ray(Line2 line, double medium)
        {   
            Line = line;
            Medium = medium;
        }
        public Ray(Vector2 point, Vector2 dir, double medium)
        {   
            Line = new Line2(dir, point);
            Medium = medium;
        }
        
        public Line2 Line { get; }
        public double Medium { get; }
    }
}