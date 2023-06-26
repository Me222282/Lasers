using System;
using System.Collections.Generic;
using Zene.Structs;

namespace Lasers
{
    public class LightingEngine
    {
        public LightingEngine()
        {
            
        }
        public LightingEngine(Box bounds)
        {
            Bounds = bounds;
        }
        
        public List<LightSource> LightSources { get; } = new List<LightSource>();
        public List<LightObject> Objects { get; } = new List<LightObject>();
        
        public Box Bounds { get; set; } = Box.One;
        public bool ReflectiveBounds { get; set; } = true;
        
        public void RenderLights(LineDC context)
        {
            for (int i = 0; i < LightSources.Count; i++)
            {
                CalculateLight(LightSources[i], context);
            }
        }
        
        public void CalculateLight(LightSource source, ICollection<LineData> lines)
        {
            ReadOnlySpan<Vector2> directions =  source.GetDirections();
            List<double> mh = new List<double>();
            
            for (int i = 0; i < directions.Length; i++)
            {
                mh.Clear();
                mh.Add(1d);
                Ray ray = new Ray(source.Location, directions[i], mh);
                CalculateRay(ray, source.Distance, source.Colour, lines);
            }
        }
        private void CalculateRay(Ray ray, double dist, ColourF3 colour, ICollection<LineData> lines)
        {
            ILightInteractable lastHit = null;
            
            ColourF c = new ColourF(colour, 1f);
            ColourF end = new ColourF(colour, 0f);
            
            while (true)
            {
                Segment2 seg = new Segment2(ray.Line, dist);
                
                ILightInteractable hit = null;
                double closeDist = dist * dist;
                Vector2 intersection = 0d;
                for (int i = 0; i < Objects.Count; i++)
                {
                    LightObject lo = Objects[i];
                    
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
                    if (!ReflectiveBounds ||
                        (seg.B.X <= Bounds.Right &&
                        seg.B.X >= Bounds.Left &&
                        seg.B.Y <= Bounds.Top &&
                        seg.B.Y >= Bounds.Bottom))
                    {
                        lines.Add(new LineData(ray.Line.Location, seg.B, c, end));
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
                lines.Add(new LineData(pointA, ray.Line.Location, c, nc));
                c = nc;
                
                if (dist <= 0d) { break; }
            }
        }
        private Line2 WallReflect(Line2 ray)
        {
            double xp = 0;
            if (ray.Direction.X > Bounds.X)
            {
                xp = Bounds.Right;
            }
            else
            {
                xp = Bounds.Left;
            }
            double yTest = ray.GetY(xp);
            
            if (yTest <= Bounds.Top &&
                yTest >= Bounds.Bottom)
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
            if (ray.Direction.Y > Bounds.Y)
            {
                yp = Bounds.Top;
            }
            else
            {
                yp = Bounds.Bottom;
            }
            
            return new Line2(
                (
                    ray.Direction.X,
                    -ray.Direction.Y
                ),
                (ray.GetX(yp), yp)
            );
        }
    }
}