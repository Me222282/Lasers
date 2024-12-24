using Zene.Structs;

namespace Lasers
{
    public struct RayArgs
    {
        public RayArgs(Segment2 ray, bool li = false)
        {
            Ray = ray;
            LastIntersect = li;
            LastRay = new Segment2(0d, 0d);
        }
        public RayArgs(Segment2 ray, Segment2 last, bool li = false)
        {
            Ray = ray;
            LastIntersect = li;
            LastRay = last;
        }
        
        public Segment2 Ray;
        public bool LastIntersect;
        public Segment2 LastRay;
    }
}