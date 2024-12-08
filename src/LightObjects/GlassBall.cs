using Zene.Structs;

namespace Lasers
{
    public class GlassBall : CircleObject
    {
        public GlassBall(Vector2 loc, double radius, double m)
            : base(new RefractCircle(loc, radius, m))
        {
            // there ust be a better way of doing this
            _fraC = (RefractCircle)Inter;
            Inter.InnerColour = new ColourF(0.7f, 0.7f, 0.7f, 0.7f);
        }
        
        private RefractCircle _fraC;
        
        public double Medium
        {
            get => _fraC.Medium;
            set => _fraC.Medium = value;
        }
    }
}