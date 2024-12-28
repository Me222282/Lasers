using System.Collections;
using System.Collections.Generic;
using Zene.Graphics;
using Zene.Structs;

namespace Lasers
{
    public class TRCreator : IEnumerator<TextureRenderer>, IEnumerable<TextureRenderer>
    {
        public TRCreator(int length, Vector2I size)
        {
            Count = length;
            Size = size;
        }
        
        public Vector2I Size;
        public int Count;
        private int _index = -1;
        
        public TextureRenderer Current { get; private set; }
        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            _index++;
            if (_index >= Count)
            {
                return false;
            }
            TextureRenderer tr = new TextureRenderer(Size.X, Size.Y);
            tr.SetColourAttachment(0, TextureFormat.R32f);
            tr.GetTexture(FrameAttachment.Colour0).DataType = TextureData.Float;
            Current = tr;
            return true;
        }

        public void Reset()
        {
            _index = -1;
        }

        public IEnumerator<TextureRenderer> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }
}