using System;
using Zene.Structs;

namespace Lasers
{
    public class RaySource : LightSource
    {
        public RaySource()
        {
            _direction = Vector2.Zero;
        }
        public RaySource(double dist, Vector2 location, Vector2 direction, ColourF3 colour)
            : base(dist, location, colour)
        {
            _direction = direction;
        }
        
        private Vector2 _direction;
        public Vector2 Direction
        {
            get => _direction;
            set => _direction = value;
        }
        
        public unsafe override ReadOnlySpan<Vector2> GetDirections()
        {
            fixed (Vector2* ptr = &_direction)
            {
                return new ReadOnlySpan<Vector2>(ptr, 1);
            }
        }
    }
}