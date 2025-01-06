using System.IO;
using Zene.Graphics;
using Zene.Structs;

namespace Lasers
{
    public class CountShader : BaseShaderProgram
    {
        public CountShader()
        {
            Create(ShaderPresets.BasicVertex, File.ReadAllText("./shaders/countFrag.glsl"), 0,
                "matrix", "value");

            SetUniform(Uniforms[0], Matrix4.Identity);
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
    }
}