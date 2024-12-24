using System;
using System.Collections;
using System.Collections.Generic;
using Zene.Structs;

namespace Lasers
{
    public abstract class PointLightSource : ILightSource
    {
        public PointLightSource()
        {
            Distance = 1d;
            Location = Vector2.Zero;
            Colour = ColourF3.Yellow;
        }
        public PointLightSource(double dist, Vector2 location, ColourF3 colour)
        {
            Distance = dist;
            Location = location;
            Colour = colour;
        }
        
        public double Distance { get; set; }
        public Vector2 Location { get; set; }
        public ColourF3 Colour { get; set; }

        public QueryData QueryMousePos(Vector2 mousePos, double range)
        {
            if (mousePos.SquaredDistance(Location) <= range * range)
            {
                return new QueryData(0, Location, this);
            }
            
            return QueryData.Fail;
        }
        public Vector2 MouseInteract(Vector2 mousePos, QueryData data)
        {
            Location = mousePos;
            return mousePos;
        }

        public abstract IEnumerator<Vector2> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}