using System.IO;
using Zene.Graphics;
using Zene.Structs;

namespace Lasers
{
    public class CountShader : BaseShaderProgram
    {
        public CountShader()
        {
            Create(ShaderPresets.BasicVertex, File.ReadAllText("./shaders/countFrag.glsl"),
                "matrix", "value");
            
            _m2m3 = new MultiplyMatrix4(null, null);
            _m1Mm2m3 = new MultiplyMatrix4(null, _m2m3);

            SetUniform(Uniforms[0], Matrix.Identity);
        }
        
        private double _value = 1d;
        public double Value
        {
            get => _value;
            set
            {
                _value = value;

                SetUniform(Uniforms[1], value);
            }
        }

        public override IMatrix Matrix1
        {
            get => _m1Mm2m3.Left;
            set => _m1Mm2m3.Left = value;
        }
        public override IMatrix Matrix2
        {
            get => _m2m3.Left;
            set => _m2m3.Left = value;
        }
        public override IMatrix Matrix3
        {
            get => _m2m3.Right;
            set => _m2m3.Right = value;
        }

        private readonly MultiplyMatrix4 _m1Mm2m3;
        private readonly MultiplyMatrix4 _m2m3;
        public override void PrepareDraw()
        {
            SetUniform(Uniforms[0], _m1Mm2m3);
        }
    }
}