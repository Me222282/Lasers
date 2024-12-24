using System.Collections.Generic;
using Zene.Structs;

namespace Lasers
{
    public interface IPointHover
    {
        public QueryData QueryMousePos(Vector2 mousePos, double range);
        public Vector2 MouseInteract(Vector2 mousePos, QueryData data);
    }
    public interface IMoveable
    {
        public bool MouseOverObject(Vector2 mousePos, double range);
        public void OffsetObjPos(Vector2 offset);
    }
    public interface ILightSource : IPointHover, IEnumerable<Vector2>
    {
        public double Distance { get; }
        public Vector2 Location { get; }
        public double Wavelength { get; }
    }
    public interface ILightObject : IPointHover, IMoveable
    {
        public int Length { get; } 
        public ILightInteractable this[int index] { get; }
        public double Medium { get; }
        
        public void Render(LineDC context);
    }
}