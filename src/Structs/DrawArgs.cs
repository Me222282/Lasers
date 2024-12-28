using System.Collections.Generic;

namespace Lasers
{
    public struct DrawArgs
    {
        public DrawArgs(ICollection<LineData> lines, double m)
        {
            Lines = lines;
            Multiplier = m;
        }
        
        public ICollection<LineData> Lines;
        public double Multiplier;
    }
}