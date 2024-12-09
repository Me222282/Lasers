using System;
using System.Collections.Generic;
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
                List<double> mh = new List<double>(){ 1d };
                Ray ray = new Ray(source.Location, d, mh);
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
            
            while (true)
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
                        
                        Vector2 inter = current.RayIntersection(seg, isLastHit);
                        
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
                    ray = new Ray(line, ray);
                    closeDist = pointA.SquaredDistance(ray.Line.Location);
                }
                else
                {
                    ray = hit.InteractRay(ray, intersection);
                }
                
                double oldDist = dist;
                dist -= Math.Sqrt(closeDist);
                
                ColourF nc = c.Lerp(end, (float)(dist / totalDist));
                // ColourF nc = c.Lerp(end, (float)Exp(dist / totalDist));
                lines.Add(new LineData(pointA, ray.Line.Location, oldC, nc));
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
        
        // private double _curvature = 5d;
        // private double _coefficient = 1d / (1d - Math.Exp(-5d));
        // private double Exp(double x)
        //     => (1 - Math.Exp(-_curvature * x)) * _coefficient;
    }
}