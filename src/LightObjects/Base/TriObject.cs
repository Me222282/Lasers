using System;
using Zene.Graphics;
using Zene.Structs;

namespace Lasers
{
    public abstract class TriObject : LightObject
    {
        public TriObject(ColourF colour, bool filled = true)
            : base(3)
        {
            if (filled)
            {
                _drawable = new DrawObject<Vector2, byte>(
                    stackalloc Vector2[1],
                    stackalloc byte[1],
                    1, 0, AttributeSize.D2, BufferUsage.DrawFrequent);
            }
            _context = State.CurrentContext;
            
            Colour = colour;
            Filled = filled;
        }
        
        protected ColourF Colour { get; set; }
        private bool _filled;
        public bool Filled
        {
            get => _filled;
            set
            {
                if (_drawable == null)
                {
                    _context.Actions.Push(() =>
                    {
                        _drawable = new DrawObject<Vector2, byte>(
                            stackalloc Vector2[1],
                            stackalloc byte[1],
                            1, 0, AttributeSize.D2, BufferUsage.DrawFrequent);
                    });
                    SetData();
                }
                
                _filled = value;
            }
        }
        
        protected abstract Vector2 _pointA { get; set; }
        protected abstract Vector2 _pointB { get; set; }
        protected abstract Vector2 _pointC { get; set; }
        
        public Vector2 PointA
        {
            get => _pointA;
            set
            {
                _pointA = value;
                SetData();
            }
        }
        public Vector2 PointB
        {
            get => _pointB;
            set
            {
                _pointB = value;
                SetData();
            }
        }
        public Vector2 PointC
        {
            get => _pointC;
            set
            {
                _pointC = value;
                SetData();
            }
        }
        
        protected void SetData()
        {
            if (!Filled) { return; }
            
            _context.Actions.Push(() =>
            {
                Span<byte> index = stackalloc byte[3];
                
                index[0] = 0;
                index[1] = 1;
                index[2] = 2;
                
                _drawable.SetData(stackalloc Vector2[]
                    {
                        _pointA,
                        _pointB,
                        _pointC
                    }, index);
            });
        }
        
        private DrawObject<Vector2, byte> _drawable;
        private BasicShader _shader = BasicShader.GetInstance();
        private GraphicsContext _context;
        
        public override void Render(LineDC context)
        {
            if (Filled)
            {
                context.Shader = _shader;
                _shader.ColourSource = ColourSource.UniformColour;
                _shader.Colour = Colour;
                context.Model = Matrix.Identity;
                context.Draw(_drawable);
            }
            
            base.Render(context);
        }
        
        public override QueryData QueryMousePos(Vector2 mousePos, double range)
        {
            range *= range;
            
            if (mousePos.SquaredDistance(_pointA) < range)
            {
                return new QueryData(0, _pointA, this);
            }
            if (mousePos.SquaredDistance(_pointB) < range)
            {
                return new QueryData(1, _pointB, this);
            }
            if (mousePos.SquaredDistance(_pointC) < range)
            {
                return new QueryData(2, _pointC, this);
            }
            
            return QueryData.Fail;
        }
        public override Vector2 MouseInteract(Vector2 mousePos, QueryData data)
        {
            base.MouseInteract(mousePos, data);
            
            if (data.Shift)
            {
                SetShift(data.PointNumber, mousePos);
                return mousePos;
            }
            
            if (data.PointNumber == 0)
            {
                PointA = mousePos;
                return mousePos;
            }
            if (data.PointNumber == 1)
            {
                PointB = mousePos;
                return mousePos;
            }
            
            PointC = mousePos;
            return mousePos;
        }
        private void SetShift(int param, Vector2 mouse)
        {
            if (param == 0)
            {
                Vector2 rp = FindOp(_pointA, _pointB, _pointC);
                _pointA = mouse;
                
                Vector2 dir = (rp - _pointA) * _sqrt3;
                
                _pointB = rp + dir.Rotated90();
                _pointC = rp + dir.Rotated270();
                SetData();
                return;
            }
            if (param == 1)
            {
                Vector2 rp = FindOp(_pointB, _pointC, _pointA);
                _pointB = mouse;
                
                Vector2 dir = (rp - _pointB) * _sqrt3;
                
                _pointC = rp + dir.Rotated90();
                _pointA = rp + dir.Rotated270();
                SetData();
                return;
            }
            else
            {
                Vector2 rp = FindOp(_pointC, _pointA, _pointB);
                _pointC = mouse;
                
                Vector2 dir = (rp - _pointC) * _sqrt3;
                
                _pointA = rp + dir.Rotated90();
                _pointB = rp + dir.Rotated270();
                SetData();
                return;
            }
        }
        private static double _sqrt3 = 1d / Math.Sqrt(3d);
        private static Vector2 FindOp(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 n1 = (b - a).Rotated90().Normalised();
            Vector2 n2 = (c - a).Rotated90().Normalised();
            
            Vector2 bi1 = n1 - n2;
            Vector2 bi2 = n1 + n2;
            
            Vector2 tp = ((b + c) / 2d) - a;
            
            Vector2 dir = bi2;
            // Bisects acute angle
            if (Math.Abs(bi1.Dot(tp)) > Math.Abs(bi2.Dot(tp)))
            {
                dir = bi1;
            }
            
            Line2 l = new Line2(dir, a);
            Line2 side3 = new Line2(b - c, c);
            return l.Intersects(side3);
        }

        public override void OffsetObjPos(Vector2 offset)
        {
            _pointA += offset;
            _pointB += offset;
            _pointC += offset;
            SetData();
        }
    }
}