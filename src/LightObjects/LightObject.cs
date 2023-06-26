using Zene.Structs;

namespace Lasers
{
    public abstract class LightObject
    {
        protected LightObject(int count)
        {
            Segments = new ILightInteractable[count];
        }
        
        protected readonly ILightInteractable[] Segments;
        
        public int Length => Segments.Length;
        public ILightInteractable this[int index] => Segments[index];
        
        public virtual void Render(LineDC context)
        {
            for (int i = 0 ; i < Segments.Length; i++)
            {
                Segments[i].Render(context);
            }
        }
        
        public abstract QueryData QueryMouseSelect(Vector2 mousePos, double range);
        public abstract void MouseInteract(Vector2 mousePos, int param);
    }
    
    public struct QueryData
    {
        public QueryData(int p, Vector2 l, ColourF3 c)
        {
            Param = p;
            Location = l;
            Colour = c;
        }
        public QueryData(bool pass)
        {
            Param = pass ? 0 : -1;
            Location = Vector2.Zero;
            Colour = ColourF3.Zero;
        }
        
        public int Param { get; }
        public Vector2 Location { get; }
        public ColourF3 Colour { get; }
        
        public bool Pass() => Param >= 0;
        
        public static QueryData Fail { get; } = new QueryData(false);
    }
}