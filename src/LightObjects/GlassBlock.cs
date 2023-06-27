using System;
using Zene.Graphics;
using Zene.Structs;

namespace Lasers
{
    public class GlassBlock : LightObject
    {
        public GlassBlock(Vector2 a, Vector2 b, Vector2 c, Vector2 d, double m)
            : base(4)
        {
            AB = new RefractPlain(a, b, m);
            BC = new RefractPlain(b, c, m);
            CD = new RefractPlain(c, d, m);
            DA = new RefractPlain(d, a, m);
            
            Segments[0] = AB;
            Segments[1] = BC;
            Segments[2] = CD;
            Segments[3] = DA;
            
            _drawable = new DrawObject<Vector2, byte>(
                stackalloc Vector2[1],
                stackalloc byte[1],
                1, 0, AttributeSize.D2, BufferUsage.DrawFrequent);
            _context = State.CurrentContext;
            
            SetData();
        }
        
        private RefractPlain AB;
        private RefractPlain BC;
        private RefractPlain CD;
        private RefractPlain DA;
        
        private Vector2 _pointA
        {
            set
            {
                AB.PointA = value;
                DA.PointB = value;
            }
        }
        public Vector2 PointA
        {
            get => AB.PointA;
            set
            {
                AB.PointA = value;
                DA.PointB = value;
                
                _context.Actions.Push(SetData);
            }
        }
        private Vector2 _pointB
        {
            set
            {
                BC.PointA = value;
                AB.PointB = value;
            }
        }
        public Vector2 PointB
        {
            get => AB.PointB;
            set
            {
                AB.PointB = value;
                BC.PointA = value;
                
                _context.Actions.Push(SetData);
            }
        }
        private Vector2 _pointC
        {
            set
            {
                CD.PointA = value;
                BC.PointB = value;
            }
        }
        public Vector2 PointC
        {
            get => CD.PointA;
            set
            {
                CD.PointA = value;
                BC.PointB = value;
                
                _context.Actions.Push(SetData);
            }
        }
        private Vector2 _pointD
        {
            set
            {
                DA.PointA = value;
                CD.PointB = value;
            }
        }
        public Vector2 PointD
        {
            get => CD.PointB;
            set
            {
                CD.PointB = value;
                DA.PointA = value;
                
                _context.Actions.Push(SetData);
            }
        }
        public double Medium
        {
            get => AB.Medium;
            set
            {
                AB.Medium = value;
                BC.Medium = value;
                CD.Medium = value;
                DA.Medium = value;
            }
        }
        
        private void SetData()
        {
            Vector2 overlap;
            
            Segment2 ab = new Segment2(AB.PointA, AB.PointB);
            Segment2 cd = new Segment2(CD.PointA, CD.PointB);
            if (ab.Intersects(cd, out overlap))
            {
                SetOverlapData(overlap, false);
                return;
            }
            
            Segment2 bc = new Segment2(BC.PointA, BC.PointB);
            Segment2 da = new Segment2(DA.PointA, DA.PointB);
            if (bc.Intersects(da, out overlap))
            {
                SetOverlapData(overlap, true);
                return;
            }
            
            Span<byte> index = stackalloc byte[6];
            
            if (AB.PointA.SquaredDistance(CD.PointA) < BC.PointA.SquaredDistance(DA.PointA))
            {
                index[0] = 0;
                index[1] = 1;
                index[2] = 2;
                index[3] = 2;
                index[4] = 3;
                index[5] = 0;
            }
            else
            {
                index[0] = 1;
                index[1] = 2;
                index[2] = 3;
                index[3] = 3;
                index[4] = 0;
                index[5] = 1;
            }
            
            _drawable.SetData(stackalloc Vector2[]
                {
                    AB.PointA,
                    BC.PointA,
                    CD.PointA,
                    DA.PointA
                }, index);
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
                    AB.PointA,
                    BC.PointA,
                    CD.PointA,
                    DA.PointA,
                    overlap
                }, index);
        }
        
        private DrawObject<Vector2, byte> _drawable;
        private BasicShader _shader = BasicShader.GetInstance();
        private GraphicsContext _context;
        
        public override void Render(LineDC context)
        {
            context.Shader = _shader;
            _shader.ColourSource = ColourSource.UniformColour;
            _shader.Colour = new ColourF(0.7f, 0.7f, 0.7f, 0.7f);
            context.Model = Matrix.Identity;
            context.Draw(_drawable);
            
            base.Render(context);
        }
        
        public override QueryData QueryMousePos(Vector2 mousePos, double range)
        {
            range *= range;
            
            if (mousePos.SquaredDistance(PointA) < range)
            {
                return new QueryData(0, PointA, this);
            }
            if (mousePos.SquaredDistance(PointB) < range)
            {
                return new QueryData(1, PointB, this);
            }
            if (mousePos.SquaredDistance(PointC) < range)
            {
                return new QueryData(2, PointC, this);
            }
            if (mousePos.SquaredDistance(PointD) < range)
            {
                return new QueryData(3, PointD, this);
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
                
                Vector2 mid = PointA.Lerp(PointC, 0.5);
                Vector2 diff = (PointA - PointC) / 2d;
                Vector2 offset1 = diff * rotate1;
                Vector2 offset2 = diff * rotate2;
                
                _pointB = mid + offset1;
                _pointD = mid + offset2;
                _context.Actions.Push(SetData);
                return;
            }
            if (param == 1)
            {
                _pointB = mouse;
                
                Vector2 mid = PointB.Lerp(PointD, 0.5);
                Vector2 diff = (PointB - PointD) / 2d;
                Vector2 offset1 = diff * rotate1;
                Vector2 offset2 = diff * rotate2;
                
                _pointA = mid + offset1;
                _pointC = mid + offset2;
                _context.Actions.Push(SetData);
                return;
            }
            if (param == 2)
            {
                _pointC = mouse;
                
                Vector2 mid = PointC.Lerp(PointA, 0.5);
                Vector2 diff = (PointC - PointA) / 2d;
                Vector2 offset1 = diff * rotate1;
                Vector2 offset2 = diff * rotate2;
                
                _pointD = mid + offset1;
                _pointB = mid + offset2;
                _context.Actions.Push(SetData);
                return;
            }
            else
            {
                _pointD = mouse;
                
                Vector2 mid = PointD.Lerp(PointB, 0.5);
                Vector2 diff = (PointD - PointB) / 2d;
                Vector2 offset1 = diff * rotate1;
                Vector2 offset2 = diff * rotate2;
                
                _pointC = mid + offset1;
                _pointA = mid + offset2;
                _context.Actions.Push(SetData);
            }
        }
        private void SetControl(int param, Vector2 mosue)
        {
            if (param == 0)
            {
                Line2 cb = new Line2(new Segment2(PointC, PointB));
                Line2 cd = new Line2(new Segment2(PointC, PointD));
                
                Line2 ab = new Line2(new Segment2(PointA, PointB));
                Line2 ad = new Line2(new Segment2(PointA, PointD));
                ab.Location = mosue;
                ad.Location = mosue;
                
                _pointA = mosue;
                
                _pointB = cb.Intersects(ab);
                _pointD = cd.Intersects(ad);
                _context.Actions.Push(SetData);
                return;
            }
            if (param == 1)
            {
                Line2 da = new Line2(new Segment2(PointD, PointA));
                Line2 dc = new Line2(new Segment2(PointD, PointC));
                
                Line2 ba = new Line2(new Segment2(PointB, PointA));
                Line2 bc = new Line2(new Segment2(PointB, PointC));
                ba.Location = mosue;
                bc.Location = mosue;
                
                _pointB = mosue;
                
                _pointA = ba.Intersects(da);
                _pointC = bc.Intersects(dc);
                _context.Actions.Push(SetData);
                return;
            }
            if (param == 2)
            {
                Line2 ab = new Line2(new Segment2(PointA, PointB));
                Line2 ad = new Line2(new Segment2(PointA, PointD));
                
                Line2 cb = new Line2(new Segment2(PointC, PointB));
                Line2 cd = new Line2(new Segment2(PointC, PointD));
                cb.Location = mosue;
                cd.Location = mosue;
                
                _pointC = mosue;
                
                _pointB = ab.Intersects(cb);
                _pointD = ad.Intersects(cd);
                _context.Actions.Push(SetData);
                return;
            }
            else
            {
                Line2 ba = new Line2(new Segment2(PointB, PointA));
                Line2 bc = new Line2(new Segment2(PointB, PointC));
                
                Line2 da = new Line2(new Segment2(PointD, PointA));
                Line2 dc = new Line2(new Segment2(PointD, PointC));
                da.Location = mosue;
                dc.Location = mosue;
                
                _pointD = mosue;
                
                _pointA = da.Intersects(ba);
                _pointC = dc.Intersects(bc);
                _context.Actions.Push(SetData);
            }
        }
    }
}