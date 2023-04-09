using System.Collections.Generic;
using System.Runtime.InteropServices;
using Zene.Graphics;
using Zene.Structs;

namespace Lasers
{
    public class LineDC : DrawManager
    {
        public unsafe LineDC()
        {
            _shader = BasicShader.GetInstance();
            _buffer = new ArrayBuffer<LineData>(1, BufferUsage.DrawFrequent);
            _dl = new DrawingLines();
            
            // Vertices
            _dl.AddBuffer(_buffer, 0, 0, sizeof(LineData) / 2, DataType.Double, AttributeSize.D2);
            // Colour
            _dl.AddBuffer(_buffer, (uint)ShaderLocation.Colour, sizeof(Vector2), sizeof(LineData) / 2, DataType.Float, AttributeSize.D4);
        }
        
        private List<LineData> _lines = new List<LineData>();
        private DrawingLines _dl;
        private ArrayBuffer<LineData> _buffer;
        private BasicShader _shader;
        
        internal void RenderCurrentLines()
        {
            _buffer.SetData(CollectionsMarshal.AsSpan(_lines));
            
            Shader = _shader;
            _shader.ColourSource = ColourSource.AttributeColour;
            Model = Matrix.Identity;
            this.Draw(_dl);
            
            _lines.Clear();
        }
        
        public void DrawLine(LineData line) => _lines.Add(line);
    }
}