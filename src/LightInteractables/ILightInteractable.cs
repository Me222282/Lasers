using Zene.Graphics;
using Zene.Structs;

namespace Lasers
{
    public interface ILightInteractable : IRenderable<DrawArgs>
    {
        public ColourF3 Colour { get; }
        
        public Vector2 RayIntersection(FindRayArgs args);
        public Ray InteractRay(ResolveRayArgs args);
    }
}