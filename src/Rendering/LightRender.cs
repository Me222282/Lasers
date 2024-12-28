using System.Collections.Generic;
using System.Threading.Tasks;
using Zene.Graphics;
using Zene.Graphics.Base;
using Zene.Structs;

namespace Lasers
{
    public class LightRender : IRenderable<DrawArgs>
    {
        public LightRender(List<LightingEngine.LSWrap> list)
        {
            _ls = list;
            
            _dc = new DrawContext()
            {
                RenderState = new RenderState()
                {
                    Blending = true,
                    SourceScaleBlending = BlendFunction.One,
                    DestinationScaleBlending = BlendFunction.One
                }
            };
            
            _shad1 = new CountShader();
            _shad2 = new DensityShader();
        }
        
        private List<LightingEngine.LSWrap> _ls;
        private List<TextureRenderer> _frames = new List<TextureRenderer>();
        
        private Vector2I _oldSize = -1;
        private LineManager _lm = new LineManager();
        private DrawContext _dc;
        
        private CountShader _shad1;
        private DensityShader _shad2;
        
        public bool Hisogram { get; set; } = true;
        
        public void OnRender(IDrawingContext context, DrawArgs args)
        {
            if (!Hisogram)
            {
                for (int i = 0; i < _ls.Count; i++)
                {
                    _lm.SetLines(_ls[i].Lines);
                    context.Render(_lm, true);
                }
                return;
            }
            
            Vector2I size = context.Framebuffer.Properties.Size;
            
            _dc.Projection = context.Projection;
            _dc.View = context.View;
            _dc.Model = context.Model;
            _dc.Shader = _shad1;
            
            // perform resize
            if (_oldSize != size)
            {
                _oldSize = size;
                for (int i = 0; i < _frames.Count; i++)
                {
                    _frames[i].Size = size;
                }
            }
            
            // match list lengths
            int count = _ls.Count - _frames.Count;
            if (_ls.Count < _frames.Count)
            {
                _frames.RemoveRange(_ls.Count, -count);
            }
            else
            {
                _frames.AddRange(new TRCreator(count, size));
            }
            
            // count lines
            float max = 0f;
            object l = new object();
            for (int i = 0; i < _ls.Count; i++)
            {
                var v = _ls[i];
                _lm.SetLines(v.Lines);
                TextureRenderer tr = _frames[i];
                tr.Clear(BufferBit.Colour);
                _dc.Framebuffer = tr;
                _shad1.Value = v.Source.Intensity;
                _dc.Render(_lm, false);
                
                GLArray<float> data = tr.GetTexture(FrameAttachment.Colour0).GetData<float>(BaseFormat.R);
                Parallel.For(0, data.Length, i =>
                {
                    float v = data[i];
                    if (v > max)
                    {
                        lock (l) { max = v; }
                    }
                });
            }
            
            // draw with opacity
            context.Shader = _shad2;
            _shad2.Div = max;
            // context.Model = Matrix4.CreateScale(args.Multiplier);
            for (int i = 0; i < _ls.Count; i++)
            {
                _shad2.Texture = _frames[i].GetTexture(FrameAttachment.Colour0);
                _shad2.Colour = _ls[i].Colour;
                context.Draw(Shapes.Square);
            }
        }
    }
}