using Zene.Structs;

namespace Lasers
{
    public class ReflectiveCircle : CircleObject
    {
        public ReflectiveCircle(Vector2 loc, double radius)
            : base(new ReflectCircle(loc, radius))
        {
            
        }
    }
}