using Zene.Structs;

namespace Lasers
{
    public interface ILightInteractable
    {
        public ColourF3 Colour { get; }
        
        public void Render(LineDC context);
        
        public Vector2 RayIntersection(RayArgs args);
        public Ray InteractRay(LightingEngine engine, Ray ray, Vector2 refPoint);
    }
}