using System;
using Zene.Structs;
using Zene.Graphics;

namespace Lasers
{
    public abstract class ArcInteractable : ILightInteractable
    {
        public ArcInteractable(Vector2 a, Vector2 b, double c)
        {
            PointA = a;
            PointB = b;
            Curve = c;
        }
        
        private Vector2 _a;
        public Vector2 PointA
        {
            get => _a;
            set
            {
                _a = value;
                Calculate();
            }
        }
        private Vector2 _b;
        public Vector2 PointB
        {
            get => _b;
            set
            {
                _b = value;
                Calculate();
            }
        }
        private double _c;
        public double Curve
        {
            get => _c;
            set
            {
                _c = value;
                Calculate();
            }
        }
        
        private bool _inverted;
        protected Vector2 _centre;
        private double _radius;
        private double _length;
        
        public Vector2 Centre => _centre;
        public double Rad => _radius;
        public Vector2 CP { get; private set; }
        
        public ColourF3 Colour => ColourF3.White;
        public ColourF InnerColour { get; set; }
        
        public void Render(LineDC context)
        {
            context.Shader = Shapes.CircleShader;
            Shapes.CircleShader.BorderColour = (ColourF)Colour;
            if (InnerColour.A != 0)
            {
                Shapes.CircleShader.ColourSource = ColourSource.UniformColour;
                Shapes.CircleShader.Colour = InnerColour;
            }
            else
            {
                Shapes.CircleShader.ColourSource = ColourSource.Discard;
            }
            
            Vector2 size = (_length, _length * _c);
            if (_c > 0.5)
            {
                size.X = _radius * 2d;
            }
            
            double hsx = size.X * 0.5;
            Box bounds = new Box(-hsx, hsx, size.Y, 0d);
            
            // Shapes.CircleShader.Size = r * 2d;
            Shapes.CircleShader.Offset = ((hsx, size.Y - _radius) / size);
            Shapes.CircleShader.SetSR(size, _radius);
            Shapes.CircleShader.LineWidth = context.Multiplier;
            
            Vector2 ab = _b - _a;
            
            double cos = ab.X / _length;
            double sin = ab.Y / _length;
            
            Matrix4 r = new Matrix4(
                new Vector4(cos, sin, 0, 0),
                new Vector4(-sin, cos, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1));
            
            context.Model = Matrix4.CreateBox(bounds) * r * Matrix4.CreateTranslation((_a + _b) / 2d);
            context.Draw(Shapes.Square);
            
            context.Model = Matrix.Identity;
        }
        
        private void Calculate()
        {
            if (_c < 0)
            {
                _c = -_c;
                Vector2 tmp = _a;
                _a = _b;
                _b = tmp;
            }
            
            _inverted = _c > 0.5;
            
            Vector2 mid = (_a + _b) / 2d;
            Vector2 t = (_a - _b);
            Vector2 dir = t.Rotated270();
            Vector2 cp = mid + (dir * _c);
            double r = _c / (2d - (t.SquaredLength / (2d * cp.SquaredDistance(_a))));
            CP = cp;
            
            _centre = cp - (r * dir);
            
            _length = dir.Length;
            _radius = r * _length;
        }
        
        public bool InSector(Vector2 v)
        {
            Vector2 cv = v - _centre;
            Vector2 ca = _a - _centre;
            Vector2 cb = _b - _centre;
            
            if (_inverted)
            {
                return !(ca.PerpDot(cv) > 0d && cb.PerpDot(cv) < 0d);
            }
            
            return ca.PerpDot(cv) <= 0d && cb.PerpDot(cv) >= 0d;
        }
        
        private const double _tolerance = 0.00001;
        
        public Vector2 RayIntersection(RayArgs args)
        {
            Segment2 ray = args.Ray;
            
            Vector2 change = ray.Change;
            Vector2 offset = ray.A - _centre;
            
            double a = change.Dot(change);
            double b = 2d * offset.Dot(change);
            double c = offset.Dot(offset) - (_radius * _radius);
            
            double discriminant = (b * b) - (4 * a * c);
            // No intersection
            if (discriminant < 0)
            {
                return Vector2.PositiveInfinity;
            }
            
            double t;
            double T = 0d;
            Vector2 p = 0d;
            Vector2 Op = 0d;
            
            if (discriminant == 0)
            {
                t = (-b / (2d * a));
            }
            else
            {
                discriminant = Math.Sqrt(discriminant);
                
                double t1 = ((discriminant - b) / (2d * a));
                double t2 = ((-discriminant - b) / (2d * a));
                
                Vector2 p1 = ray.A + (t1 * change);
                // if (!InSector(p1))
                // {
                //     t1 = -1d;
                // }
                Vector2 p2 = ray.A + (t2 * change);
                // if (!InSector(p2))
                // {
                //     t2 = -1d;
                // }
                
                if (t1 <= 0d)
                {
                    t = t2;
                    p = p2;
                    T = t1;
                    Op = p1;
                }
                else if (t2 <= 0d)
                {
                    t = t1;
                    p = p1;
                    T = t2;
                    Op = p2;
                }
                else
                {
                    t = Math.Min(t1, t2);
                    T = Math.Max(t1, t2);
                    if (t == t1)
                    {
                        p = p1;
                        Op = p2;
                    }
                    else
                    {
                        p = p2;
                        Op = p1;
                    }
                }
            }
            
            if (t > 1d || t <= 0d)
            {
                return Vector2.PositiveInfinity;
            }
            
            // No rereflect issues
            if (!args.LastIntersect)
            {
                if (InSector(p))
                {
                    return p;
                }
                if (T > 1d || T <= 0d)
                {
                    return Vector2.PositiveInfinity;
                }
                if (InSector(Op))
                {
                    return Op;
                }
                return Vector2.PositiveInfinity;
            }
            
            bool inside = (ray.A - _centre).Dot(args.LastRay.Change) > 0d;
            
            // heading away or there are 2 points
            if (!inside || T > 0d)
            {
                t = -1d;
            }
            
            // Use other intersection
            if (t <= 0d)
            {
                if (T > 0d)
                {
                    if (!InSector(Op))
                    {
                        return Vector2.PositiveInfinity;
                    }
                    return Op;
                }
                
                return Vector2.PositiveInfinity;
            }
            
            if (!InSector(p))
            {
                return Vector2.PositiveInfinity;
            }
            return p;
        }
        
        public abstract Ray InteractRay(LightingEngine engine, Ray ray, Vector2 refPoint);
    }
}