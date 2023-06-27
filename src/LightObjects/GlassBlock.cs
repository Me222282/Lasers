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
                stackalloc Vector2[] { a, b, c, d },
                stackalloc byte[] { 0, 1, 2, 2, 3, 0 },
                1, 0, AttributeSize.D2, BufferUsage.DrawFrequent);
            _context = State.CurrentContext;
        }
        
        private RefractPlain AB;
        private RefractPlain BC;
        private RefractPlain CD;
        private RefractPlain DA;
        
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
            _drawable.SetData(stackalloc Vector2[]
                {
                    AB.PointA,
                    BC.PointA,
                    CD.PointA,
                    DA.PointA
                });
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
                Set(data.PointNumber, mousePos, data.Scroll);
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
        private void Set(int param, Vector2 mouse, double scroll)
        {
            Matrix2 rotate1 = Matrix2.CreateRotation(Radian.Quarter + (scroll * 0.03));
            Matrix2 rotate2 = Matrix2.CreateRotation(-Radian.Quarter + (scroll * 0.03));
            
            if (param == 0)
            {
                PointA = mouse;
                
                Vector2 mid = PointA.Lerp(PointC, 0.5);
                Vector2 diff = (PointA - PointC) / 2d;
                Vector2 offset1 = diff * rotate1;
                Vector2 offset2 = diff * rotate2;
                
                PointB = mid + offset1;
                PointD = mid + offset2;
                return;
            }
            if (param == 1)
            {
                PointB = mouse;
                
                Vector2 mid = PointB.Lerp(PointD, 0.5);
                Vector2 diff = (PointB - PointD) / 2d;
                Vector2 offset1 = diff * rotate1;
                Vector2 offset2 = diff * rotate2;
                
                PointA = mid + offset1;
                PointC = mid + offset2;
                return;
            }
            if (param == 2)
            {
                PointC = mouse;
                
                Vector2 mid = PointC.Lerp(PointA, 0.5);
                Vector2 diff = (PointC - PointA) / 2d;
                Vector2 offset1 = diff * rotate1;
                Vector2 offset2 = diff * rotate2;
                
                PointD = mid + offset1;
                PointB = mid + offset2;
                return;
            }
            else
            {
                PointD = mouse;
                
                Vector2 mid = PointD.Lerp(PointB, 0.5);
                Vector2 diff = (PointD - PointB) / 2d;
                Vector2 offset1 = diff * rotate1;
                Vector2 offset2 = diff * rotate2;
                
                PointC = mid + offset1;
                PointA = mid + offset2;
            }
        }
    }
}