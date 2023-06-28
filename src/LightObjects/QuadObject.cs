using System;
using Zene.Graphics;
using Zene.Structs;

namespace Lasers
{
    public abstract class QuadObject : LightObject
    {
        public QuadObject(int count, ColourF colour, bool filled = true)
            : base(count)
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
        protected abstract Vector2 _pointD { get; set; }
        
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
        public Vector2 PointD
        {
            get => _pointD;
            set
            {
                _pointD = value;
                SetData();
            }
        }
        
        protected void SetData()
        {
            if (!Filled) { return; }
            
            Vector2 overlap;
            
            Segment2 ab = new Segment2(_pointA, _pointB);
            Segment2 cd = new Segment2(_pointC, _pointD);
            if (ab.Intersects(cd, out overlap))
            {
                _tr0 = new Triangle(_pointB, PointA, overlap);
                _tr1 = new Triangle(_pointD, PointC, PointA);
                _context.Actions.Push(() => SetOverlapData(overlap, false));
                return;
            }
            
            Segment2 bc = new Segment2(_pointB, _pointC);
            Segment2 da = new Segment2(_pointD, _pointA);
            if (bc.Intersects(da, out overlap))
            {
                _tr0 = new Triangle(_pointD, PointA, overlap);
                _tr1 = new Triangle(_pointC, PointB, overlap);
                _context.Actions.Push(() => SetOverlapData(overlap, true));
                return;
            }
            
            Line2 ac = new Line2(new Segment2(_pointA, _pointC));
            bool b = ac.GetY(_pointB.X) > _pointB.Y;
            bool d = ac.GetY(_pointD.X) > _pointD.Y;
            
            if (b == d)
            {
                _tr0 = new Triangle(_pointB, PointC, _pointD);
                _tr1 = new Triangle(_pointD, PointA, _pointB);
            }
            else
            {
                _tr0 = new Triangle(_pointA, PointB, _pointC);
                _tr1 = new Triangle(_pointC, PointD, _pointA);
            }
            
            _context.Actions.Push(() =>
            {
                Span<byte> index = stackalloc byte[6];
                
                // On same side of ac line
                if (b == d)
                {
                    index[0] = 1;
                    index[1] = 2;
                    index[2] = 3;
                    index[3] = 3;
                    index[4] = 0;
                    index[5] = 1;
                }
                else
                {
                    index[0] = 0;
                    index[1] = 1;
                    index[2] = 2;
                    index[3] = 2;
                    index[4] = 3;
                    index[5] = 0;
                }
                
                _drawable.SetData(stackalloc Vector2[]
                    {
                        _pointA,
                        _pointB,
                        _pointC,
                        _pointD
                    }, index);
            });
        }
        private unsafe void SetOverlapData(Vector2 overlap, bool option)
        {
            Span<byte> index = stackalloc byte[6];
            
            if (option)
            {
                index[0] = 0;
                index[1] = 1;
                index[2] = 4;
                index[3] = 2;
                index[4] = 3;
                index[5] = 4;
            }
            else
            {
                index[0] = 0;
                index[1] = 3;
                index[2] = 4;
                index[3] = 1;
                index[4] = 2;
                index[5] = 4;
            }
            
            _drawable.SetData(stackalloc Vector2[]
                {
                    _pointA,
                    _pointB,
                    _pointC,
                    _pointD,
                    overlap
                }, index);
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
            if (mousePos.SquaredDistance(_pointD) < range)
            {
                return new QueryData(3, _pointD, this);
            }
            
            return QueryData.Fail;
        }
        public override void MouseInteract(Vector2 mousePos, QueryData data)
        {
            base.MouseInteract(mousePos, data);
            
            if (data.Shift)
            {
                SetShift(data.PointNumber, mousePos, data.Scroll);
                return;
            }
            if (data.Control)
            {
                SetControl(data.PointNumber, mousePos);
                return;
            }
            
            if (data.PointNumber == 0)
            {
                PointA = mousePos;
                return;
            }
            if (data.PointNumber == 1)
            {
                PointB = mousePos;
                return;
            }
            if (data.PointNumber == 2)
            {
                PointC = mousePos;
                return;
            }
            
            PointD = mousePos;
        }
        private void SetShift(int param, Vector2 mouse, double scroll)
        {
            Matrix2 rotate1 = Matrix2.CreateRotation(Radian.Quarter + (scroll * 0.03));
            Matrix2 rotate2 = Matrix2.CreateRotation(-Radian.Quarter + (scroll * 0.03));
            
            if (param == 0)
            {
                _pointA = mouse;
                
                Vector2 mid = _pointA.Lerp(_pointC, 0.5);
                Vector2 diff = (_pointA - _pointC) / 2d;
                Vector2 offset1 = diff * rotate1;
                Vector2 offset2 = diff * rotate2;
                
                _pointB = mid + offset1;
                _pointD = mid + offset2;
                SetData();
                return;
            }
            if (param == 1)
            {
                _pointB = mouse;
                
                Vector2 mid = _pointB.Lerp(_pointD, 0.5);
                Vector2 diff = (_pointB - _pointD) / 2d;
                Vector2 offset1 = diff * rotate1;
                Vector2 offset2 = diff * rotate2;
                
                _pointA = mid + offset1;
                _pointC = mid + offset2;
                SetData();
                return;
            }
            if (param == 2)
            {
                _pointC = mouse;
                
                Vector2 mid = _pointC.Lerp(_pointA, 0.5);
                Vector2 diff = (_pointC - _pointA) / 2d;
                Vector2 offset1 = diff * rotate1;
                Vector2 offset2 = diff * rotate2;
                
                _pointD = mid + offset1;
                _pointB = mid + offset2;
                SetData();
                return;
            }
            else
            {
                _pointD = mouse;
                
                Vector2 mid = _pointD.Lerp(_pointB, 0.5);
                Vector2 diff = (_pointD - _pointB) / 2d;
                Vector2 offset1 = diff * rotate1;
                Vector2 offset2 = diff * rotate2;
                
                _pointC = mid + offset1;
                _pointA = mid + offset2;
                SetData();
            }
        }
        private void SetControl(int param, Vector2 mosue)
        {
            if (param == 0)
            {
                Line2 cb = new Line2(new Segment2(_pointC, _pointB));
                Line2 cd = new Line2(new Segment2(_pointC, _pointD));
                
                Line2 ab = new Line2(new Segment2(_pointA, _pointB));
                Line2 ad = new Line2(new Segment2(_pointA, _pointD));
                ab.Location = mosue;
                ad.Location = mosue;
                
                _pointA = mosue;
                
                _pointB = cb.Intersects(ab);
                _pointD = cd.Intersects(ad);
                SetData();
                return;
            }
            if (param == 1)
            {
                Line2 da = new Line2(new Segment2(_pointD, _pointA));
                Line2 dc = new Line2(new Segment2(_pointD, _pointC));
                
                Line2 ba = new Line2(new Segment2(_pointB, _pointA));
                Line2 bc = new Line2(new Segment2(_pointB, _pointC));
                ba.Location = mosue;
                bc.Location = mosue;
                
                _pointB = mosue;
                
                _pointA = ba.Intersects(da);
                _pointC = bc.Intersects(dc);
                SetData();
                return;
            }
            if (param == 2)
            {
                Line2 ab = new Line2(new Segment2(_pointA, _pointB));
                Line2 ad = new Line2(new Segment2(_pointA, _pointD));
                
                Line2 cb = new Line2(new Segment2(_pointC, _pointB));
                Line2 cd = new Line2(new Segment2(_pointC, _pointD));
                cb.Location = mosue;
                cd.Location = mosue;
                
                _pointC = mosue;
                
                _pointB = ab.Intersects(cb);
                _pointD = ad.Intersects(cd);
                SetData();
                return;
            }
            else
            {
                Line2 ba = new Line2(new Segment2(_pointB, _pointA));
                Line2 bc = new Line2(new Segment2(_pointB, _pointC));
                
                Line2 da = new Line2(new Segment2(_pointD, _pointA));
                Line2 dc = new Line2(new Segment2(_pointD, _pointC));
                da.Location = mosue;
                dc.Location = mosue;
                
                _pointD = mosue;
                
                _pointA = da.Intersects(ba);
                _pointC = dc.Intersects(bc);
                SetData();
            }
        }
        
        private Triangle _tr0;
        private Triangle _tr1;
        protected override void AddOffset(Vector2 offset)
        {
            _pointA += offset;
            _pointB += offset;
            _pointC += offset;
            _pointD += offset;
            SetData();
        }
        public override bool MouseOverObject(Vector2 mousePos)
        {
            if (Filled)
            {
                return _tr0.ContainsPoint(mousePos) || _tr1.ContainsPoint(mousePos);
            }
            
            return false;
        }
        
        private struct Triangle
        {
            public Triangle(Vector2 a, Vector2 b, Vector2 c)
            {
                A = a;
                B = b;
                C = c;
            }
            
            public Vector2 A;
            public Vector2 B;
            public Vector2 C;
            
            private double Sign(Vector2 p1, Vector2 p2, Vector2 p3)
            {
                return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
            }
            public bool ContainsPoint(Vector2 p)
            {
                double d1, d2, d3;
                bool hasNeg, hasPos;

                d1 = Sign(p, A, B);
                d2 = Sign(p, B, C);
                d3 = Sign(p, C, A);

                hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
                hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

                return !(hasNeg && hasPos);
            }
        }
    }
}