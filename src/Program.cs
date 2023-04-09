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
        internal struct LinePoint
        {
            public LinePoint(Vector2 l, ColourF c)
            {
                Location = l;
                Colour = c;
            }
            public LinePoint(double x, double y, float r, float g, float b)
            {
                Location = new Vector2(x, y);
                Colour = new ColourF(r, g, b);
            }
            public LinePoint(double x, double y, float r, float g, float b, float a)
            {
                Location = new Vector2(x, y);
                Colour = new ColourF(r, g, b, a);
            }
            
            public Vector2 Location { get; set; }
            public ColourF Colour { get; set; }
        }
        
        public Program()
        {
            Graphics = new LocalGraphics(this, OnRender);
            Layout = new Layout(0d, 0d, 2d, 2d);
            
            Random r = new Random();
            
            _context = new LineDC();
            _mirrors = new Mirror[10];
            for (int i = 0; i < _mirrors.Length; i++)
            {
                _mirrors[i] = new Mirror(r.NextVector2(2) - 1d, r.NextVector2(2) - 1d);
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
            p.Run();
            
            Core.Terminate();
        }
        
        private Mirror[] _mirrors;
        private LineDC _context;
        
        private void OnRender(object sender, RenderArgs e)
        {
            e.Context.Framebuffer.Clear(new ColourF(0.1f, 0.1f, 0.1f));
            
            RenderRay(_context);
            
            for (int i = 0; i < _mirrors.Length; i++)
            {
                _mirrors[i].Render(_context);
            }
            
            _context.Framebuffer = e.Context.Framebuffer;
            _context.Model = Matrix.Identity;
            _context.View = Matrix.Identity;
            _context.Projection = Matrix.Identity;
            _context.RenderCurrentLines();
        }
        
        private Ray _startRay;
        private double _distance = 10d;
        
        private void RenderRay(LineDC context)
        {
            ILightInteractable lastHit = null;
            
            Ray ray = _startRay;
            double dist = _distance;
            ColourF c = Colour.Yellow;
            ColourF end = new ColourF(ColourF3.Yellow, 0f);
            
            while (true)
            {
                ILightInteractable hit = null;
                double closeDist = dist * dist;
                Vector2 intersection = 0d;
                for (int i = 0; i < _mirrors.Length; i++)
                {
                    ILightInteractable current = _mirrors[i].Segments[0];
                    
                    if (current == lastHit) { continue; }
                    
                    Vector2 inter = current.RayIntersection(new Segment2(ray.Line, dist));
                    
                    double currentDist = inter.SquaredDistance(ray.Line.Location);
                    if (currentDist < closeDist)
                    {
                        intersection = inter;
                        closeDist = currentDist;
                        hit = current;
                    }
                }
                lastHit = hit;
                
                if (closeDist == dist * dist)
                {
                    Segment2 temp = new Segment2(ray.Line, dist);
                    context.DrawLine(new LineData(ray.Line.Location, temp.B, c, end));
                    break;
                }
                
                Vector2 pointA = ray.Line.Location;
                ray = hit.InteractRay(ray, intersection);
                double oldDist = dist;
                dist -= pointA.Distance(ray.Line.Location);
                
                ColourF nc = end.Lerp(c, (float)(dist / oldDist));
                context.DrawLine(new LineData(pointA, ray.Line.Location, c, nc));
                c = nc;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e[Keys.R])
            {
                Random r = new Random();
                
                for (int i = 0; i < _mirrors.Length; i++)
                {
                    _mirrors[i] = new Mirror(r.NextVector2(2) - 1d, r.NextVector2(2) - 1d);
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
                
                Vector2 dir = (mouse - refPoint).Normalised();
                
                _startRay = new Ray(refPoint, dir, _startRay);
            }
        }
    }
}
