using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Zene.Graphics;
using Zene.Structs;

namespace Lasers
{
    public class LightingEngine
    {
        public class LSWrap
        {
            public ILightSource Source;
            public List<LineData> Lines;
            public ColourF3 Colour;
            public double WL;
        }
        private class LSWrapCast : IList<ILightSource>
        {
            public LSWrapCast(List<LSWrap> s) => Source = s;
            public List<LSWrap> Source;
            public ILightSource this[int index] { get => Source[index].Source; set => Source[index].Source = value; }
            public int Count => Source.Count;
            public bool IsReadOnly => false;
            public void Add(ILightSource item)
            {
                Source.Add(new LSWrap()
                {
                    Source = item,
                    Lines = new List<LineData>()
                });
            }
            public void Clear() => Source.Clear();
            public bool Contains(ILightSource item) => Source.Exists(p => p.Source == item);
            public void CopyTo(ILightSource[] array, int arrayIndex) => throw new NotSupportedException();
            public IEnumerator<ILightSource> GetEnumerator() => new EnumerCast(Source.GetEnumerator());
            public int IndexOf(ILightSource item) => Source.FindIndex(p => p.Source == item);
            public void Insert(int index, ILightSource item)
            {
                Source.Insert(index, new LSWrap()
                {
                    Source = item,
                    Lines = new List<LineData>()
                });
            }
            public bool Remove(ILightSource item)
            {
                int i = IndexOf(item);
                if (i < 0) { return false; }
                Source.RemoveAt(i);
                return true;
            }
            public void RemoveAt(int index) => Source.RemoveAt(index);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        private class EnumerCast : IEnumerator<ILightSource>
        {
            public EnumerCast(IEnumerator<LSWrap> s) => Source = s;
            public IEnumerator<LSWrap> Source;
            public ILightSource Current => Source.Current.Source;
            object IEnumerator.Current => Source.Current.Source;
            public void Dispose() => Source.Dispose();
            public bool MoveNext() => Source.MoveNext();
            public void Reset() => Source.Reset();
        }
        
        public LightingEngine()
        {
            LightSources = new LSWrapCast(_ls);
        }
        public LightingEngine(Box bounds)
            : this()
        {
            Bounds = bounds;
        }
        
        private List<LSWrap> _ls = new List<LSWrap>();
        private List<TextureRenderer> _frames = new List<TextureRenderer>();
        public IList<ILightSource> LightSources { get; }
        public List<ILightObject> Objects { get; } = new List<ILightObject>();
        
        public Box Bounds { get; set; } = Box.One;
        public bool ReflectiveBounds { get; set; } = true;
        public double AirMedium { get; set; } = 1d;
        
        // public int Bounces { get; set; } = 1;
        
        public LightRender CreateRender() => new LightRender(_ls);
        
        public void CalculateRays()
        {
            Parallel.ForEach(_ls, (ls) =>
            {
                ls.Lines.Clear();
                double wl = ls.Source.Wavelength;
                if (wl != ls.WL)
                {
                    ls.WL = wl;
                    ls.Colour = ColourF3.FromWavelength((float)wl);
                }
                CalculateLight(ls.Source, ls.Lines, ls.Colour);
            });
        }
        
        public void CalculateLight(ILightSource source, ICollection<LineData> lines, ColourF3 c = default)
        {
            if (c == default)
            {
                c = ColourF3.FromWavelength((float)source.Wavelength);
            }
            
            Parallel.ForEach(source, (d) =>
            {
                double m = GetMedium(source.Location, null);
                Ray ray = new Ray(source.Location, d, m);
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
                        lock (lines)
                        {
                            lines.Add(new LineData(ray.Line.Location, seg.B, oldC, end));
                        }
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
                lock (lines)
                {
                    lines.Add(new LineData(lastSeg.A, lastSeg.B, oldC, nc));
                }
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