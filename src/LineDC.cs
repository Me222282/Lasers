using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Zene.Graphics;
using Zene.Structs;

namespace Lasers
{
    public class LineDC : DrawManager, ICollection<LineData>
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

        int ICollection<LineData>.Count => _lines.Count;
        bool ICollection<LineData>.IsReadOnly => false;

        internal void RenderLines()
        {
            lock (_lines)
            {
                _buffer.SetData(CollectionsMarshal.AsSpan(_lines));
                
                Shader = _shader;
                _shader.ColourSource = ColourSource.AttributeColour;
                Model = Matrix.Identity;
                this.Draw(_dl);
            }
        }
        
        public void AddLine(LineData line)
        {
            lock (_lines)
            {
                _lines.Add(line);
            }
        }
        public void ClearLines()
        {
            lock (_lines)
            {
                _lines.Clear();
            }
        }

        void ICollection<LineData>.Add(LineData item)
        {
            lock (_lines)
            {
                _lines.Add(item);
            }
        }
        void ICollection<LineData>.Clear()
        {
            lock (_lines)
            {
                _lines.Clear();
            }
        }
        bool ICollection<LineData>.Contains(LineData item)
        {
            lock (_lines)
            {
                return _lines.Contains(item);
            }
        }
        void ICollection<LineData>.CopyTo(LineData[] array, int arrayIndex)
        {
            lock (_lines)
            {
                _lines.CopyTo(array, arrayIndex);
            }
        }
        bool ICollection<LineData>.Remove(LineData item)
        {
            lock (_lines)
            {
                return _lines.Remove(item);
            }
        }
        IEnumerator<LineData> IEnumerable<LineData>.GetEnumerator()
        {
            lock (_lines)
            {
                return _lines.GetEnumerator();
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_lines)
            {
                return _lines.GetEnumerator();
            }
        }
    }
}