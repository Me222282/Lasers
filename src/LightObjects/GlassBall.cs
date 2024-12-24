using Zene.Structs;

namespace Lasers
{
    public class GlassBall : CircleObject<RefractCircle>
    {
        public GlassBall(Vector2 loc, double radius, double m)
            : base(new RefractCircle(loc, radius, m))
        {
            Inter.InnerColour = new ColourF(0.7f, 0.7f, 0.7f, 0.7f);
        }
        
        public override double Medium
        {
            get => Inter.Medium;
            set => Inter.Medium = value;
        }
    }
}