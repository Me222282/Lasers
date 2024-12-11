using System;
using Zene.Structs;
using Zene.Graphics;

namespace Lasers
{
    public class ReflectArc : ILightInteractable
    {
        public ReflectArc(Vector2 a, Vector2 b, double c)
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
        private Vector2 _centre;
        private double _radius;
        
        public ColourF3 Colour => ColourF3.White;
        
        public void Render(LineDC context)
        {
            // context.AddLine(new LineData(PointA, PointB, ColourF.White));
            // context.DrawArc(PointA, PointB, Curve, context.Multiplier, (ColourF)Colour);
            
            context.Shader = Shapes.CircleShader;
            Shapes.CircleShader.BorderColour = (ColourF)Colour;
            Shapes.CircleShader.ColourSource = ColourSource.Discard;
            
            Box bounds = new Box(_centre, 2d * _radius);
            
            Vector2 a = _a, b = _b;
            
            if (b.X > a.X)
            {
                bounds.Bottom = Math.Min(a.Y, b.Y);
            }
            else if (b.X < a.X)
            {
                bounds.Top = Math.Max(a.Y, b.Y);
            }
            if (b.Y > a.Y)
            {
                bounds.Right = Math.Max(a.X, b.X);
            }
            else if (b.Y < a.Y)
            {
                bounds.Left = Math.Min(a.X, b.X);
            }
            
            // Shapes.CircleShader.Size = r * 2d;
            Vector2 size = (bounds.Width, bounds.Height);
            Shapes.CircleShader.Offset = (_centre - (bounds.Left, bounds.Bottom)) / size;
            Shapes.CircleShader.SetSR(size, _radius);
            Shapes.CircleShader.LineWidth = context.Multiplier;
            
            context.Model = Matrix4.CreateBox(bounds);
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
            
            _centre = cp - (r * dir);
            
            _radius = r * dir.Length;
        }
        
        private bool InSector(Vector2 v)
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
        
        public Vector2 RayIntersection(Segment2 ray, bool lastIntersect)
        {
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
                if (!InSector(p1))
                {
                    t1 = -1d;
                }
                Vector2 p2 = ray.A + (t2 * change);
                if (!InSector(p2))
                {
                    t2 = -1d;
                }
                
                if (t1 < 0d)
                {
                    t = t2;
                    p = p2;
                    T = t1;
                    Op = p1;
                }
                else if (t2 < 0d)
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
            
            if (t > 1d || t < 0d)
            {
                return Vector2.PositiveInfinity;
            }
            
            // No rereflect issues
            if (!lastIntersect)
            {
                return p;
            }
            
            // Heading away from circle centre - detection is a rereflect
            if (offset.SquaredLength < _centre.SquaredDistance(ray.A + (change * _tolerance * _radius / change.SquaredLength)))
            {
                return Vector2.PositiveInfinity;
            }
            
            if (T <= 0d)
            {
                return p;
            }
            
            // Use other intersection
            return Op;
        }
        
        public Ray InteractRay(LightingEngine engine, Ray ray, Vector2 refPoint)
        {
            Vector2 diff = _centre - refPoint;
            Line2 reflect = new Line2(diff, refPoint);
            Vector2 np = reflect.Reflect(ray.Line.Location);
            
            return new Ray(refPoint, (np - refPoint).Normalised(), ray.Medium);
        }
    }
}