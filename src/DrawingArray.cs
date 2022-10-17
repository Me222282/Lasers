using Zene.Graphics.Base;
using Zene.Graphics;

namespace Lasers
{
    public unsafe class DrawingArray : VertexArrayGL
    {
        public void AddBuffer<T>(ArrayBuffer<T> buffer, uint index, int dataStart, DataType dataType, AttributeSize attributeSize) where T : unmanaged
        {
            int typeSize = sizeof(T);

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
            buffer.Bind();
            EnableVertexAttribArray(index);
            if (GL.Version >= 3.3)
            {
                VertexAttribDivisor(index, 0);
            }
            VertexAttribPointer(index, attributeSize, dataType, false, stride, dataStart);
        }

        public void Draw(DrawMode mode, int first, int size) => DrawArrays(mode, first, size);
        public void Draw<T>(DrawMode mode, int index) where T : unmanaged
        {
            ArrayBuffer<T> buffer = (ArrayBuffer<T>)Properties.GetBuffer(0);
            
            DrawArrays(mode, 0, buffer.Size);
        }
    }
}