using System;
using System.Collections.Generic;
using Zene.Structs;

namespace Lasers
{
    public abstract class LightSource : IPointHover
    {
        public LightSource()
        {
            Distance = 1d;
            Location = Vector2.Zero;
            Colour = ColourF3.Yellow;
        }
        public LightSource(double dist, Vector2 location, ColourF3 colour)
        {
            Distance = dist;
            Location = location;
            Colour = colour;
        }
        
        public double Distance { get; set; }
        public Vector2 Location { get; set; }
        public ColourF3 Colour { get; set; }
        
        public abstract IEnumerable<Vector2> GetDirections();

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
    }
}