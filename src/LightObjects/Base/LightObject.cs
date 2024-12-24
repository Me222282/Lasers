using System;
using System.Collections.Generic;
using Zene.Structs;

namespace Lasers
{
    public abstract class LightObject
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
        
        private Vector2 _lastMousePos;
        
        public virtual void Render(LineDC context)
        {
            for (int i = 0 ; i < Segments.Length; i++)
            {
                Segments[i].Render(context);
            }
        }
        
        protected abstract void AddOffset(Vector2 offset);
        public abstract QueryData QueryMousePos(Vector2 mousePos, double range);
        public virtual Vector2 MouseInteract(Vector2 mousePos, QueryData data)
        {
            LastScrollOffset = data.Scroll;
            return mousePos;
        }
        public bool MouseOverObject(Vector2 mousePos, double range)
        {
            _lastMousePos = mousePos;
            return IsMouseOverObject(mousePos, range);
        }
        protected internal virtual bool IsMouseOverObject(Vector2 mousePos, double range)
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
        public void SetObjPos(Vector2 pos)
        {
            AddOffset(pos - _lastMousePos);
            _lastMousePos = pos;
        }
    }
    
    public struct QueryData
    {
        public QueryData(int p, Vector2 l, LightObject source)
        {
            PointNumber = p;
            Location = l;
            Source = source;
            Scroll = source.LastScrollOffset;
            Shift = false;
            Control = false;
        }
        
        public int PointNumber { get; }
        public Vector2 Location { get; }
        public LightObject Source { get; }
        public double Scroll { get; set; }
        
        public bool Shift { get; set; }
        public bool Control { get; set; }
        
        public bool Pass() => Source != null;

        public override bool Equals(object obj)
        {
            return obj is QueryData data &&
                   Source == data.Source &&
                   PointNumber == data.PointNumber;
        }
        public override int GetHashCode() => HashCode.Combine(PointNumber, Location, Source);

        public static QueryData Fail { get; } = new QueryData();
        
        public static bool operator ==(QueryData a, QueryData b) =>  a.Equals(b);
        public static bool operator !=(QueryData a, QueryData b) =>  !a.Equals(b);
    }
}