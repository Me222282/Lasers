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
                
                _drawable.SetData(stackalloc Vector2[]
                    {
                        AB.PointA,
                        BC.PointA,
                        CD.PointA,
                        DA.PointA
                    });
            }
        }
        public Vector2 PointB
        {
            get => AB.PointB;
            set
            {
                AB.PointB = value;
                BC.PointA = value;
                
                _drawable.SetData(stackalloc Vector2[]
                    {
                        AB.PointA,
                        BC.PointA,
                        CD.PointA,
                        DA.PointA
                    });
            }
        }
        public Vector2 PointC
        {
            get => CD.PointA;
            set
            {
                CD.PointA = value;
                BC.PointB = value;
                
                _drawable.SetData(stackalloc Vector2[]
                    {
                        AB.PointA,
                        BC.PointA,
                        CD.PointA,
                        DA.PointA
                    });
            }
        }
        public Vector2 PointD
        {
            get => CD.PointB;
            set
            {
                CD.PointB = value;
                DA.PointA = value;
                
                _drawable.SetData(stackalloc Vector2[]
                    {
                        AB.PointA,
                        BC.PointA,
                        CD.PointA,
                        DA.PointA
                    });
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
        
        private DrawObject<Vector2, byte> _drawable;
        private BasicShader _shader = BasicShader.GetInstance();
        
        public override void Render(LineDC context)
        {
            context.Shader = _shader;
            _shader.ColourSource = ColourSource.UniformColour;
            _shader.Colour = new ColourF(0.7f, 0.7f, 0.7f, 0.7f);
            context.Model = Matrix.Identity;
            context.Draw(_drawable);
            
            base.Render(context);
        }
    }
}