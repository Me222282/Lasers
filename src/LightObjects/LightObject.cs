namespace Lasers
{
    public abstract class LightObject
    {
        protected LightObject(int count)
        {
            Segments = new ILightInteractable[count];
        }
        
        public ILightInteractable[] Segments { get; }
        
        public virtual void Render(LineDC context)
        {
            for (int i = 0 ; i < Segments.Length; i++)
            {
                Segments[i].Render(context);
            }
        }
    }
}