using Zene.Structs;

namespace Lasers
{
    public abstract class LineObject : LightObject
    {
        public LineObject(int count)
            : base(count)
        {
            
        }
        
        public abstract Vector2 PointA { get; set; }
        public abstract Vector2 PointB { get; set; }
        
        public override QueryData QueryMousePos(Vector2 mousePos, double range)
        {
            range *= range;
            
            if (mousePos.SquaredDistance(PointA) < range)
            {
                return new QueryData(0, PointA, this);
            }
            if (mousePos.SquaredDistance(PointB) < range)
            {
                return new QueryData(1, PointB, this);
            }
            
            return QueryData.Fail;
        }
        public override void MouseInteract(Vector2 mousePos, QueryData data)
        {
            base.MouseInteract(mousePos, data);
            
            if (data.PointNumber == 0)
            {
                PointA = mousePos;
                return;
            }
            
            PointB = mousePos;
        }
        
        protected override void AddOffset(Vector2 offset)
        {
            PointA += offset;
            PointB += offset;
        }
        public override bool MouseOverObject(Vector2 mousePos, double range)
        {
            base.MouseOverObject(mousePos, range);
            
            Segment2 seg = new Segment2(PointA, PointB);
            Vector2 dir = seg.Change;
            dir = (Vector2)(-dir.Y, dir.X) * 1000d;
            Segment2 perp = new Segment2(mousePos - dir, mousePos + dir);
            
            if (!seg.Intersects(perp, out Vector2 i)) { return false; }
            
            return mousePos.SquaredDistance(i) <= (range * range);
        }
    }
}