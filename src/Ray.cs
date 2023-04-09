using Zene.Structs;

namespace Lasers
{
    public struct Ray
    {
        public Ray(Line2 line, double m)
        {
            Line = line;
            Medium = m;
        }
        public Ray(Line2 line, Ray source)
        {   
            Line = line;
            Medium = source.Medium;
        }
        public Ray(Vector2 point, Vector2 dir, Ray source)
        {   
            Line = new Line2(dir, point);
            Medium = source.Medium;
        }
        
        public Line2 Line { get; }
        public double Medium { get; }
    }
}