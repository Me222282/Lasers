using System;
using System.Collections.Generic;
using Zene.Structs;

namespace Lasers
{
    public class DisperseRays : LightSource
    {
        public DisperseRays()
        {
            _direction = Vector2.Zero;
            _range = TwoPI;
            RayCount = 4;
        }
        
        private Vector2 _direction;
        public Vector2 Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                CalculateDirections();
            }
        }
        
        private int _rayCount;
        public int RayCount
        {
            get => _rayCount;
            set
            {
                _rayCount = value;
                _directions = new Vector2[_rayCount];
                CalculateDirections();
            }
        }
        
        private const double TwoPI = Math.PI * 2d;
        private Radian _range;
        public Radian Range
        {
            get => _range;
            set
            {
                _range = Math.Abs(value);
                if (_range > TwoPI)
                {
                    _range = TwoPI;
                }
                CalculateDirections();
            }
        }
        
        private Vector2[] _directions;
        public override IEnumerable<Vector2> GetDirections() => _directions;
        
        private void CalculateDirections()
        {
            Radian angle = Math.Atan2(_direction.Y, _direction.X);
            angle -= _range / 2d;
            Radian division = _range / _rayCount;
            
            for (int i = 0; i < _rayCount; i++)
            {
                _directions[i] = new Vector2(Math.Cos(angle), Math.Sin(angle));
                angle += division;
            }
        }
    }
}