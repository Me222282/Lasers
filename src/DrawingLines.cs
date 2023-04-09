using Zene.Graphics.Base;
using Zene.Graphics;

namespace Lasers
{
    public unsafe class DrawingLines : VertexArrayGL, IDrawObject
    {
        private int _drawBufferSize = 0;
        
        public void AddBuffer<T>(ArrayBuffer<T> buffer, uint index, int dataStart, DataType dataType, AttributeSize attributeSize) where T : unmanaged
        {   
            int typeSize = sizeof(T);
            
            if (index == 0)
            {
                _drawBufferSize = typeSize;
            }
            
            buffer.Bind();
            EnableVertexAttribArray(index);
            if (GL.Version >= 3.3)
            {
                VertexAttribDivisor(index, 0);
            }
            VertexAttribPointer(index, attributeSize, dataType, false, typeSize * (int)buffer.DataSplit, dataStart * typeSize);
        }
        public void AddBuffer<T>(ArrayBuffer<T> buffer, uint index, int dataStart, int stride, DataType dataType, AttributeSize attributeSize) where T : unmanaged
        {
            if (index == 0)
            {
                _drawBufferSize = stride;
            }
            
            buffer.Bind();
            EnableVertexAttribArray(index);
            if (GL.Version >= 3.3)
            {
                VertexAttribDivisor(index, 0);
            }
            VertexAttribPointer(index, attributeSize, dataType, false, stride, dataStart);
        }

        public Drawable GetRenderable(IDrawingContext context)
        {
            IBuffer buffer = Properties.GetBuffer(0);
            
            return new Drawable(this,
                new RenderInfo(
                    DrawMode.Lines, 0,
                    buffer.Properties.Size / _drawBufferSize
                ));
        }
    }
}