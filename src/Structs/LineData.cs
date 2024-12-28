using Zene.Structs;

namespace Lasers
{
    public struct LineData
    {
        public LineData(Vector2 a, Vector2 b, ColourF c)
        {
            PointA = a;
            PointB = b;
            ColourA = c;
            ColourB = c;
        }
        public LineData(Vector2 a, Vector2 b, ColourF colourA, ColourF colourB)
        {
            PointA = a;
            PointB = b;
            ColourA = colourA;
            ColourB = colourB;
        }
        public LineData(Segment2 seg, ColourF c)
        {
            PointA = seg.A;
            PointB = seg.B;
            ColourA = c;
            ColourB = c;
        }
        public LineData(Segment2 seg, ColourF colourA, ColourF colourB)
        {
            PointA = seg.A;
            PointB = seg.B;
            ColourA = colourA;
            ColourB = colourB;
        }
        
        public Vector2 PointA { get; set; }
        public ColourF ColourA { get; set; }
        
        public Vector2 PointB { get; set; }
        public ColourF ColourB { get; set; }
    }
}