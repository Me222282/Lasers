using Zene.Structs;

namespace Lasers
{
    public class GlassBall : CircleObject
    {
        public GlassBall(Vector2 loc, double radius, double m)
        // Again this is annoying
            : base(null)
        {
            _fraC = new RefractCircle(loc, radius, m, this);
            Inter = _fraC;
            Segments[0] = _fraC;
            
            Inter.InnerColour = new ColourF(0.7f, 0.7f, 0.7f, 0.7f);
        }
        
        private RefractCircle _fraC;
        
        public override double Medium
        {
            get => _fraC.Medium;
            set => _fraC.Medium = value;
        }
    }
}