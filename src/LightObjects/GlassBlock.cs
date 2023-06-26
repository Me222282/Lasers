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
        
        public override QueryData QueryMouseSelect(Vector2 mousePos, double range)
        {
            range *= range;
            
            if (mousePos.SquaredDistance(PointA) < range)
            {
                return new QueryData(0, PointA, AB.Colour);
            }
            if (mousePos.SquaredDistance(PointB) < range)
            {
                return new QueryData(1, PointB, BC.Colour);
            }
            if (mousePos.SquaredDistance(PointC) < range)
            {
                return new QueryData(2, PointC, CD.Colour);
            }
            if (mousePos.SquaredDistance(PointD) < range)
            {
                return new QueryData(3, PointD, DA.Colour);
            }
            
            return QueryData.Fail;
        }
        public override void MouseInteract(Vector2 mousePos, int param)
        {
            if (param == 0)
            {
                PointA = mousePos;
                return;
            }
            if (param == 1)
            {
                PointB = mousePos;
                return;
            }
            if (param == 2)
            {
                PointC = mousePos;
                return;
            }
            
            PointD = mousePos;
        }
    }
}