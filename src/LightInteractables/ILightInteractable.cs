using Zene.Structs;

namespace Lasers
{
    public interface ILightInteractable
    {
        public ColourF3 Colour { get; }
        
        public void Render(LineDC context);
        
        public Vector2 RayIntersection(FindRayArgs args);
        public Ray InteractRay(ResolveRayArgs args);
    }
}