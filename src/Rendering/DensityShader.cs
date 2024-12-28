using System.IO;
using Zene.Graphics;
using Zene.Structs;

namespace Lasers
{
    public class DensityShader : BaseShaderProgram
    {
        public DensityShader()
        {
            Create(ShaderPresets.BasicVertex, File.ReadAllText("./shaders/densityFrag.glsl"),
                "matrix", "uTextureSlot", "div", "uColour");
            
            // _m2m3 = new MultiplyMatrix4(null, null);
            // _m1Mm2m3 = new MultiplyMatrix4(null, _m2m3);
            
            SetUniform(Uniforms[0], Matrix4.CreateScale(2d));
            SetUniform(Uniforms[1], 0);
        }
        
        private ColourF3 _colour = ColourF3.Zero;
        public ColourF3 Colour
        {
            get => _colour;
            set
            {
                _colour = value;

                SetUniform(Uniforms[3], (Vector3)value);
            }
        }
        
        private double _div = 1d;
        public double Div
        {
            get => _div;
            set
            {
                _div = value;

                SetUniform(Uniforms[2], value);
            }
        }

        public ITexture Texture { get; set; }

        // public override IMatrix Matrix1
        // {
        //     get => _m1Mm2m3.Left;
        //     set => _m1Mm2m3.Left = value;
        // }
        // public override IMatrix Matrix2
        // {
        //     get => _m2m3.Left;
        //     set => _m2m3.Left = value;
        // }
        // public override IMatrix Matrix3
        // {
        //     get => _m2m3.Right;
        //     set => _m2m3.Right = value;
        // }

        // private readonly MultiplyMatrix4 _m1Mm2m3;
        // private readonly MultiplyMatrix4 _m2m3;
        public override void PrepareDraw()
        {
            // SetUniform(Uniforms[0], _m1Mm2m3);
            Texture?.Bind(0);
        }
    }
}