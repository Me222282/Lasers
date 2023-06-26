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
        
        public int Length => Segments.Length;
        public ILightInteractable this[int index] => Segments[index];
        
        public virtual void Render(LineDC context)
        {
            for (int i = 0 ; i < Segments.Length; i++)
            {
                Segments[i].Render(context);
            }
        }
        
        public abstract QueryData QueryMouseSelect(Vector2 mousePos, double range);
        public abstract void MouseInteract(Vector2 mousePos, ref QueryData data);
    }
    
    public struct QueryData
    {
        public QueryData(int p, Vector2 l, ColourF3 c, ILightInteractable source)
        {
            Param = p;
            Location = l;
            Colour = c;
            Source = source;
            Shift = false;
            Control = false;
        }
        
        public int Param { get; }
        public Vector2 Location { get; set; }
        public ColourF3 Colour { get; set; }
        public ILightInteractable Source { get; }
        
        public bool Shift { get; set; }
        public bool Control { get; set; }
        
        public bool Pass() => Source != null;

        public override bool Equals(object obj)
        {
            return obj is QueryData data &&
                   Source == data.Source;
        }
        public override int GetHashCode() => HashCode.Combine(Param, Location, Colour, Source);

        public static QueryData Fail { get; } = new QueryData();
        
        public static bool operator ==(QueryData a, QueryData b) =>  a.Equals(b);
        public static bool operator !=(QueryData a, QueryData b) =>  !a.Equals(b);
    }
}