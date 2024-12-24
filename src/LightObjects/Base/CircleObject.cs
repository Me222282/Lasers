using System;
using Zene.Structs;

namespace Lasers
{
    public abstract class CircleObject<T> : ILightObject
        where T : CircleInteractable
    {
        public CircleObject(T i)
        {
            Inter = i;
        }
        
        protected T Inter;
        
        public int Length => 1;
        public ILightInteractable this[int index] => Inter;
        public virtual double Medium { get; set; } = -1d;
        
        public Vector2 Location
        {
            get => Inter.Location;
            set => Inter.Location = value;
        }
        public double Radius
        {
            get => Inter.Radius;
            set => Inter.Radius = value;
        }
        
        public virtual void Render(LineDC context) => Inter.Render(context);
        
        public QueryData QueryMousePos(Vector2 mousePos, double range)
        {   
            double dist = mousePos.SquaredDistance(Inter.Location);
            double rMin = Inter.Radius - range;
            double rMax = Inter.Radius + range;
            
            if (dist < (rMin * rMin) ||
                dist > (rMax * rMax))
            {
                return QueryData.Fail;
            }
            
            Vector2 diff = mousePos - Inter.Location;
            diff *= Inter.Radius / Math.Sqrt(dist);
            
            return new QueryData(0, Inter.Location + diff, this);
        }
        public Vector2 MouseInteract(Vector2 mousePos, QueryData data)
        {
            Inter.Radius = Inter.Location.Distance(mousePos);
            return mousePos;
        }
        
        public void OffsetObjPos(Vector2 offset) => Inter.Location += offset;
        public bool MouseOverObject(Vector2 mousePos, double range)
        {
            double dist = mousePos.SquaredDistance(Inter.Location);
            double r2 = Inter.Radius * Inter.Radius;
            
            return dist <= r2;
        }
    }
}