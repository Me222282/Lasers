using Zene.Structs;

namespace Lasers
{
    public class Mirror : LightObject
    {
        public Mirror(Vector2 a, Vector2 b)
            : base(1)
        {
            Segments[0] = new ReflectPlain(a, b);
        }
        
        public Vector2 PointA
        {
            get => ((ReflectPlain)Segments[0]).PointA;
            set
            {
                ReflectPlain rp = (ReflectPlain)Segments[0];
                Segments[0] = new ReflectPlain(value, rp.PointB);
            }
        }
        public Vector2 PointB
        {
            get => ((ReflectPlain)Segments[0]).PointB;
            set
            {
                ReflectPlain rp = (ReflectPlain)Segments[0];
                Segments[0] = new ReflectPlain(rp.PointA, value);
            }
        }
    }
}