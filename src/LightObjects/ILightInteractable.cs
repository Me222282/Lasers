using Zene.Structs;

namespace Lasers
{
    public interface ILightInteractable
    {
        public ColourF3 Colour { get; }
        
        public void Render(LineDC context);
        
        public Vector2 RayIntersection(Segment2 ray);
        public Ray InteractRay(Ray ray, Vector2 refPoint);
    }
}