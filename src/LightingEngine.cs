using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Zene.Graphics;
using Zene.Structs;

namespace Lasers
{
    public class LightingEngine : IRenderable<DrawArgs>
    {
        public LightingEngine()
        {
            
        }
        public LightingEngine(Box bounds)
        {
            Bounds = bounds;
        }
        
        public List<ILightSource> LightSources { get; } = new List<ILightSource>();
        public List<ILightObject> Objects { get; } = new List<ILightObject>();
        
        
        public Box Bounds { get; set; } = Box.One;
        public bool ReflectiveBounds { get; set; } = true;
        public double AirMedium { get; set; } = 1d;
        
        // public int Bounces { get; set; } = 1;
        
        public void OnRender(IDrawingContext context, DrawArgs args)
        {
            Parallel.ForEach(LightSources, (ls) =>
            {
                CalculateLight(ls, args.Lines);
            });
        }
        
        public void CalculateLight(ILightSource source, ICollection<LineData> lines)
        {
            Parallel.ForEach(source, (d) =>
            {
                double m = GetMedium(source.Location, null);
                Ray ray = new Ray(source.Location, d, m);
                ColourF3 c = ColourF3.FromWavelength((float)source.Wavelength);
                CalculateRay(ray, source.Distance, c, lines);
            });
            
        }
        private void CalculateRay(Ray ray, double dist, ColourF3 colour, ICollection<LineData> lines)
        {
            ILightInteractable lastHit = null;
            
            ColourF c = new ColourF(colour, 1f);
            ColourF end = new ColourF(colour, 0f);
            ColourF oldC = c;
            
            double totalDist = dist;
            
            Segment2 lastSeg = new Segment2(0d, 0d);
            
            while (true)
            // for (int j = 0; j < Bounces; j++)
            {
                Segment2 seg = new Segment2(ray.Line, dist);
                
                ILightInteractable hit = null;
                ILightObject hitSource = null;
                double closeDist = dist * dist;
                Vector2 intersection = 0d;
                for (int i = 0; i < Objects.Count; i++)
                {
                    ILightObject lo = Objects[i];
                    
                    for (int l = 0; l < lo.Length; l++)
                    {
                        ILightInteractable current = lo[l];
                        
                        bool isLastHit = current == lastHit;
                        
                        Vector2 inter = current.RayIntersection(new FindRayArgs(seg, isLastHit));
                        
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
                            hitSource = lo;
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
                    ray = hit.InteractRay(new ResolveRayArgs(this, ray, intersection, hitSource));
                }
                
                double oldDist = dist;
                dist -= Math.Sqrt(closeDist);
                
                lastSeg.A = pointA;
                lastSeg.B = ray.Line.Location;
                
                ColourF nc = end.Lerp(c, (float)(dist / totalDist));
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
        
        public double GetMedium(Vector2 point, ILightObject ignore)
        {
            ReadOnlySpan<ILightObject> span = CollectionsMarshal.AsSpan(Objects);
            for (int i = 0; i < span.Length; i++)
            {
                ILightObject lo = span[i];
                if (lo == ignore || lo.Medium == -1 ||
                // Test
                    !lo.PointOverObject(point, 0d))
                {
                    continue;
                }
                
                return lo.Medium;
            }
            
            return AirMedium;
        }
    }
}