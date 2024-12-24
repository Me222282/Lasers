using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
        public double AirMedium { get; set; } = 1d;
        
        public int Bounces { get; set; } = 1;
        
        public void RenderLights(LineDC context)
        {
            Parallel.ForEach(LightSources, (ls) =>
            {
                CalculateLight(ls, context);
            });
        }
        
        public void CalculateLight(LightSource source, ICollection<LineData> lines)
        {
            IEnumerable<Vector2> directions =  source.GetDirections();
            
            Parallel.ForEach(directions, (d) =>
            {
                double m = GetMedium(source.Location, null);
                Ray ray = new Ray(source.Location, d, m);
                CalculateRay(ray, source.Distance, source.Colour, lines);
            });
            /*
            for (int i = 0; i < directions.Length; i++)
            {
                mh.Clear();
                mh.Add(1d);
                Ray ray = new Ray(source.Location, directions[i], mh);
                CalculateRay(ray, source.Distance, source.Colour, lines);
            }*/
        }
        private void CalculateRay(Ray ray, double dist, ColourF3 colour, ICollection<LineData> lines)
        {
            ILightInteractable lastHit = null;
            
            ColourF c = new ColourF(colour, 1f);
            ColourF end = new ColourF(colour, 0f);
            ColourF oldC = c;
            
            double totalDist = dist;
            
            Segment2 lastSeg = new Segment2(0d, 0d);
            
            // while (true)
            for (int j = 0; j < Bounces; j++)
            {
                Segment2 seg = new Segment2(ray.Line, dist);
                
                ILightInteractable hit = null;
                double closeDist = dist * dist;
                Vector2 intersection = 0d;
                for (int i = 0; i < Objects.Count; i++)
                {
                    LightObject lo = Objects[i];
                    
                    for (int l = 0; l < lo.Length; l++)
                    {
                        ILightInteractable current = lo[l];
                        
                        bool isLastHit = current == lastHit;
                        
                        Vector2 inter = current.RayIntersection(new RayArgs(seg, lastSeg, isLastHit));
                        
                        // Intersection outside of bounds
                        if (ReflectiveBounds &&
                            (inter.X > Bounds.Right ||
                            inter.X < Bounds.Left ||
                            inter.Y > Bounds.Top ||
                            inter.Y < Bounds.Bottom))
                        {
                            continue;
                        }
                        
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
                    // Reaches end of ray without hitting wall
                    if (!ReflectiveBounds ||
                        (seg.B.X <= Bounds.Right &&
                        seg.B.X >= Bounds.Left &&
                        seg.B.Y <= Bounds.Top &&
                        seg.B.Y >= Bounds.Bottom))
                    {
                        lines.Add(new LineData(ray.Line.Location, seg.B, oldC, end));
                        break;
                    }
                    
                    // Reflect off walls
                    Line2 line = WallReflect(ray.Line);
                    ray = new Ray(line, ray.Medium);
                    closeDist = pointA.SquaredDistance(ray.Line.Location);
                }
                else
                {
                    ray = hit.InteractRay(this, ray, intersection);
                }
                
                double oldDist = dist;
                dist -= Math.Sqrt(closeDist);
                
                lastSeg.A = pointA;
                lastSeg.B = ray.Line.Location;
                
                ColourF nc = end.Lerp(c, (float)(dist / totalDist));
                // ColourF nc = end.Lerp(c, (float)Exp(dist / totalDist));
                lines.Add(new LineData(lastSeg.A, lastSeg.B, oldC, nc));
                oldC = nc;
                
                if (dist <= 0d || ray.Line.Direction == Vector2.Zero) { break; }
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
        
        public double GetMedium(Vector2 point, LightObject ignore)
        {
            ReadOnlySpan<LightObject> span = CollectionsMarshal.AsSpan(Objects);
            for (int i = 0; i < span.Length; i++)
            {
                LightObject lo = span[i];
                if (lo == ignore || lo.Medium == -1 ||
                // Test
                    !lo.IsMouseOverObject(point, 0d))
                {
                    continue;
                }
                
                return lo.Medium;
            }
            
            return AirMedium;
        }
        
        // private double _curvature = 5d;
        // private double _coefficient = 1d / (1d - Math.Exp(-5d));
        // private double Exp(double x)
        //     => (1 - Math.Exp(-_curvature * x)) * _coefficient;
    }
}