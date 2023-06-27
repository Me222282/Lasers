using System;
using System.IO;
using Zene.Graphics;
using Zene.Structs;

namespace Lasers
{
    public class PostShader : BaseShaderProgram
    {
        public PostShader()
        {
            Create(File.ReadAllText("resources/VertexShaderP.glsl"), File.ReadAllText("resources/FragmentShaderP.glsl"),
                "uTextureSlot", "kernel", "kernelOffset");
        }

        private int _ts;
        public int TextureSlot
        {
            get => _ts;
            set
            {
                _ts = value;
                SetUniform(Uniforms[0], value);
            }
        }
        
        private double[] _kernel;
        public double[] Kernel
        {
            get => _kernel;
            set
            {
                if (value.Length != 9)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _kernel = value;

                SetUniform(Uniforms[1], value);
            }
        }

        private double _kernelOffset;
        public double KernelOffset
        {
            get => _kernelOffset;
            set
            {
                _kernelOffset = value;

                SetUniform(Uniforms[2], value);
            }
        }

        public static double[] BlurKernel { get; } = new double[]
        {
            1.0 / 16, 2.0 / 16, 1.0 / 16,
            2.0 / 16, 4.0 / 16, 2.0 / 16,
            1.0 / 16, 2.0 / 16, 1.0 / 16
        };

        public static double[] SharpenKernel { get; } = new double[]
        {
            -1, -1, -1,
            -1, 9, -1,
            -1, -1, -1
        };
    }
}