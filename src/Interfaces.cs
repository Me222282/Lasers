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
}