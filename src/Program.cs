using System;
using Zene.Windowing;
using Zene.Graphics;
using Zene.Structs;
using System.Collections.Generic;
using System.IO;
using Zene.GUI;

namespace Lasers
{
    class Program : Element
    {
        public Program()
        {
            Graphics = new LocalGraphics(this, OnRender);
            Layout = new Layout(0d, 0d, 2d, 2d);
            
            Random r = new Random();
            
            _context = new LineDC();
            _objects = new LightObject[10];
            for (int i = 0; i < _objects.Length; i++)
            {
                _objects[i] = new Mirror(r.NextVector2(2) - 1d, r.NextVector2(2) - 1d);
            }
            
            _startRay = new Ray(new Line2(r.NextVector2(2) - 1d, r.NextVector2(2) - 1d), 0d);
        }
        
        public override GraphicsManager Graphics { get; }

        static void Main(string[] args)
        {
            Core.Init();
            
            GUIWindow p = new GUIWindow(800, 500, "LIGHT");
            p.AddChild(new Program());
            p.RootElement.ShiftFocusRight();
            //p.Run();
            p.RunMultithread();
            
            Core.Terminate();
        }
        
        private LightObject[] _objects;
        private LineDC _context;
        
        private void OnRender(object sender, RenderArgs e)
        {
            e.Context.Framebuffer.Clear(new ColourF(0.1f, 0.1f, 0.1f));
            
            RenderRay(_context);
            
            for (int i = 0; i < _objects.Length; i++)
            {
                _objects[i].Render(_context);
            }
            
            _context.Framebuffer = e.Context.Framebuffer;
            _context.Model = Matrix.Identity;
            _context.View = Matrix.Identity;
            _context.Projection = Projection();
            _context.DrawBox(_bounds, ColourF.DarkCyan);
            _context.RenderCurrentLines();
        }
        
        private Ray _startRay;
        private double _distance = 10d;
        private Box _bounds = new Box(-1d, 1d, 1d, -1d);
        
        private void RenderRay(LineDC context)
        {
            ILightInteractable lastHit = null;
            
            Ray ray = _startRay;
            double dist = _distance;
            ColourF c = Colour.Yellow;
            ColourF end = new ColourF(ColourF3.Yellow, 0f);
            
            while (true)
            {
                Segment2 seg = new Segment2(ray.Line, dist);
                
                ILightInteractable hit = null;
                double closeDist = dist * dist;
                Vector2 intersection = 0d;
                for (int i = 0; i < _objects.Length; i++)
                {
                    LightObject lo = _objects[i];
                    
                    for (int l = 0; l < lo.Segments.Length; l++)
                    {
                        ILightInteractable current = lo.Segments[l];
                        
                        if (current == lastHit) { continue; }
                        
                        Vector2 inter = current.RayIntersection(seg);
                        
                        double currentDist = inter.SquaredDistance(ray.Line.Location);
                        if (currentDist < closeDist)
                        {
                            intersection = inter;
                            closeDist = currentDist;
                            hit = current;
                        }
                    }
                }
                lastHit = hit;
                
                Vector2 pointA = ray.Line.Location;
                
                if (closeDist == dist * dist)
                {
                    if (seg.B.X <= _bounds.Right &&
                        seg.B.X >= _bounds.Left &&
                        seg.B.Y <= _bounds.Top &&
                        seg.B.Y >= _bounds.Bottom)
                    {
                        context.DrawLine(new LineData(ray.Line.Location, seg.B, c, end));
                        break;
                    }
                    
                    Line2 line = WallReflect(ray.Line);
                    ray = new Ray(line, ray);
                }
                else
                {
                    ray = hit.InteractRay(ray, intersection);
                }
                
                double oldDist = dist;
                dist -= pointA.Distance(ray.Line.Location);
                
                ColourF nc = end.Lerp(c, (float)(dist / oldDist));
                context.DrawLine(new LineData(pointA, ray.Line.Location, c, nc));
                c = nc;
                
                if (dist <= 0d) { break; }
            }
        }
        private Line2 WallReflect(Line2 ray)
        {
            double xp = 0;
            if (ray.Direction.X > _bounds.X)
            {
                xp = _bounds.Right;
            }
            else
            {
                xp = _bounds.Left;
            }
            double yTest = ray.GetY(xp);
            
            if (yTest <= _bounds.Top &&
                yTest >= _bounds.Bottom)
            {
                return new Line2(
                    (
                        -ray.Direction.X,
                        ray.Direction.Y
                    ),
                    (xp, yTest)
                );
            }
            
            double yp = 0;
            if (ray.Direction.Y > _bounds.Y)
            {
                yp = _bounds.Top;
            }
            else
            {
                yp = _bounds.Bottom;
            }
            
            return new Line2(
                (
                    ray.Direction.X,
                    -ray.Direction.Y
                ),
                (ray.GetX(yp), yp)
            );
        }
        
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e[Keys.R])
            {
                Random r = new Random();
                
                for (int i = 0; i < _objects.Length; i++)
                {
                    _objects[i] = new Mirror(r.NextVector2(2) - 1d, r.NextVector2(2) - 1d);
                }
                
                _startRay = new Ray(new Line2(r.NextVector2(2) - 1d, r.NextVector2(2) - 1d), 0d);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            if (this[Keys.Space])
            {
                Vector2 refPoint = _startRay.Line.Location;
                Vector2 mouse = e.Location / (Size * 0.5d);
                mouse *= _renderScale;
                
                Vector2 dir = (mouse - refPoint).Normalised();
                
                _startRay = new Ray(refPoint, dir, _startRay);
            }
        }
        
        private Vector2 _renderScale = Vector2.One;
        private IMatrix Projection()
        {
            double winWidth = Size.X;
            double winHeight = Size.Y;

            double w;
            double h;

            if (winHeight > winWidth)
            {
                w = _bounds.Width;
                h = (winHeight / winWidth) * _bounds.Width;
            }
            else // Width is bigger
            {
                h = _bounds.Height;
                w = (winWidth / (double)winHeight) * _bounds.Height;
            }
            
            _renderScale = (w * 0.5, h * 0.5);
            return Matrix4.CreateOrthographic(w, h, -1d, 1d);
        }
    }
}
