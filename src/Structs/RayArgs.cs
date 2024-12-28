using Zene.Structs;

namespace Lasers
{
    public struct FindRayArgs
    {
        public FindRayArgs(Segment2 ray, bool li = false)
        {
            Ray = ray;
            LastIntersect = li;
            // LastRay = new Segment2(0d, 0d);
        }
        // public RayArgs(Segment2 ray, Segment2 last, bool li = false)
        // {
        //     Ray = ray;
        //     LastIntersect = li;
        //     LastRay = last;
        // }
        
        public Segment2 Ray;
        public bool LastIntersect;
        //public Segment2 LastRay;
    }
    public struct ResolveRayArgs
    {
        public ResolveRayArgs(LightingEngine e, Ray r, Vector2 p, ILightObject s)
        {
            Engine = e;
            Ray = r;
            Point = p;
            Source = s;
        }
        
        public LightingEngine Engine;
        public Ray Ray;
        public Vector2 Point;
        public ILightObject Source;
    }
}