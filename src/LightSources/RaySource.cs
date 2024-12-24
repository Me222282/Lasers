using System;
using System.Collections;
using System.Collections.Generic;
using Zene.Structs;

namespace Lasers
{
    public class RaySource : PointLightSource
    {
        public RaySource()
        {
            _direction = Vector2.Zero;
        }
        public RaySource(double dist, Vector2 location, Vector2 direction, double wl)
            : base(dist, location, wl)
        {
            _direction = direction;
        }
        
        private Vector2 _direction;
        public Vector2 Direction
        {
            get => _direction;
            set => _direction = value;
        }
        
        public override IEnumerator<Vector2> GetEnumerator()
        {
            yield return _direction;
        }
    }
}