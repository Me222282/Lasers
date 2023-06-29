using System;
using Zene.Structs;

namespace Lasers
{
    public abstract class CircleObject : LightObject
    {
        public CircleObject(CircleInteractable i)
            : base(1)
        {
            Inter = i;
            Segments[0] = Inter;
        }
        
        protected CircleInteractable Inter;
        
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
        
        public override QueryData QueryMousePos(Vector2 mousePos, double range)
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
        public override void MouseInteract(Vector2 mousePos, QueryData data)
        {
            base.MouseInteract(mousePos, data);
            
            Inter.Radius = Inter.Location.Distance(mousePos);
        }
        
        protected override void AddOffset(Vector2 offset) => Inter.Location += offset;
        public override bool MouseOverObject(Vector2 mousePos, double range)
        {
            base.MouseOverObject(mousePos, range);
            
            double dist = mousePos.SquaredDistance(Inter.Location);
            double r2 = Inter.Radius * Inter.Radius;
            
            return dist <= r2;
        }
    }
}