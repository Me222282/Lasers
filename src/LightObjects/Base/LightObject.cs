using System;
using System.Collections.Generic;
using Zene.Structs;

namespace Lasers
{
    public abstract class LightObject : IPointHover, IMoveable
    {
        protected LightObject(int count)
        {
            Segments = new ILightInteractable[count];
        }
        
        protected readonly ILightInteractable[] Segments;
        
        public double LastScrollOffset { get; private set; } = 0d;
        public int Length => Segments.Length;
        public ILightInteractable this[int index] => Segments[index];
        public virtual double Medium { get; set; } = -1d;
        
        public virtual void Render(LineDC context)
        {
            for (int i = 0 ; i < Segments.Length; i++)
            {
                Segments[i].Render(context);
            }
        }
        
        public abstract QueryData QueryMousePos(Vector2 mousePos, double range);
        public virtual Vector2 MouseInteract(Vector2 mousePos, QueryData data)
        {
            LastScrollOffset += data.Scroll;
            return mousePos;
        }
        public virtual bool MouseOverObject(Vector2 mousePos, double range)
        {
            Segment2 cast = new Segment2(mousePos, new Vector2(mousePos.X + 1000_000d, mousePos.Y));
            
            int count = 0;
            for (int i = 0; i < Segments.Length; i++)
            {
                if (!double.IsInfinity(Segments[i].RayIntersection(new RayArgs(cast)).X))
                {
                    count++;
                }
            }
            
            return count % 2 == 1;
        }
        public abstract void OffsetObjPos(Vector2 offset);
    }
}