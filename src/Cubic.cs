using System;
using System.Collections.Generic;
using System.Linq;
using Zene.Structs;

namespace Lasers
{
    public static class Cubic
    {
        internal static Program.LinePoint[] InterpolateXY(List<Program.LinePoint> p, int m)
        {
            if (p.Count % 2 != 0)
            {
                return p.ToArray();
            }
            
            int lc = p.Count * m;
            
            Program.LinePoint[] newLines = new Program.LinePoint[lc];
            
            (double[] ix, double[] iy) = Convert(p);
            (double[] xs, double[] ys) = InterpolateXY(ix, iy, (lc / 2) + 1);
            
            newLines[0] = new Program.LinePoint(
                (xs[0], ys[0]),
                p[0].Colour
            );
            
            double di = 1d / m;
            double pi = di;
            int ni = 1;
            for (int i = 1; i < (xs.Length - 1); i++)
            {
                Vector2 v = (xs[i], ys[i]);
                
                newLines[ni] = new Program.LinePoint(
                    v, ReadLerpValue(p, pi)
                );
                newLines[ni + 1] = new Program.LinePoint(
                    v, ReadLerpValue(p, pi)
                );
                
                pi += di;
                ni += 2;
            }
            
            newLines[lc - 1] = new Program.LinePoint(
                (xs[xs.Length - 1], ys[ys.Length - 1]),
                p[p.Count - 1].Colour
            );
            
            return newLines;
        }
        private static (double[] xs, double[] ys) Convert(List<Program.LinePoint> p)
        {
            double[] x = new double[p.Count / 2];
            double[] y = new double[p.Count / 2];
            
            x[0] = p[0].Location.X;
            y[0] = p[0].Location.Y;
            
            int ni = 1;
            for (int i = 1; i < (p.Count - 1); i += 2)
            {
                x[ni] = p[i].Location.X;
                y[ni] = p[i].Location.Y;
                
                ni++;
            }
            
            //x[ni] = p[p.Count - 1].Location.X;
            //y[ni] = p[p.Count - 1].Location.Y;
            
            return (x, y);
        }
        private static ColourF ReadLerpValue(List<Program.LinePoint> l, double i)
        {
            double a = l[(int)i].Colour.A;
            float b = l[((int)i) + 1].Colour.A;
            
            //return Lerp(a, b, (float)i - ((int)i));
            return new ColourF(
                0f, 0.5f, 0.8f,
                (float)a.Lerp(b, i - ((int)i))
            );
        }
        private static ColourF Lerp(ColourF a, ColourF b, float l)
        {
            return new ColourF(
                a.R + ((b.R - a.R) * l),
                a.G + ((b.B - a.B) * l),
                a.B + ((b.G - a.G) * l),
                a.A + ((b.A - a.A) * l)
            );
        }
        
        /// <summary>
        /// Generate a smooth (interpolated) curve that follows the path of the given X/Y points
        /// </summary>
        public static (double[] xs, double[] ys) InterpolateXY(double[] xs, double[] ys, int count)
        {
            if (xs is null || ys is null || xs.Length != ys.Length)
                throw new ArgumentException($"{nameof(xs)} and {nameof(ys)} must have same length");

            int inputPointCount = xs.Length;
            double[] inputDistances = new double[inputPointCount];
            for (int i = 1; i < inputPointCount; i++)
            {
                double dx = xs[i] - xs[i - 1];
                double dy = ys[i] - ys[i - 1];
                double distance = Math.Sqrt(dx * dx + dy * dy);
                inputDistances[i] = inputDistances[i - 1] + distance;
            }

            double meanDistance = inputDistances.Last() / (count - 1);
            double[] evenDistances = Enumerable.Range(0, count).Select(x => x * meanDistance).ToArray();
            double[] xsOut = Interpolate(inputDistances, xs, evenDistances);
            double[] ysOut = Interpolate(inputDistances, ys, evenDistances);
            return (xsOut, ysOut);
        }

        private static double[] Interpolate(double[] xOrig, double[] yOrig, double[] xInterp)
        {
            (double[] a, double[] b) = FitMatrix(xOrig, yOrig);

            double[] yInterp = new double[xInterp.Length];
            for (int i = 0; i < yInterp.Length; i++)
            {
                int j;
                for (j = 0; j < xOrig.Length - 2; j++)
                    if (xInterp[i] <= xOrig[j + 1])
                        break;

                double dx = xOrig[j + 1] - xOrig[j];
                double t = (xInterp[i] - xOrig[j]) / dx;
                double y = (1 - t) * yOrig[j] + t * yOrig[j + 1] +
                    t * (1 - t) * (a[j] * (1 - t) + b[j] * t);
                yInterp[i] = y;
            }

            return yInterp;
        }

        private static (double[] a, double[] b) FitMatrix(double[] x, double[] y)
        {
            int n = x.Length;
            double[] a = new double[n - 1];
            double[] b = new double[n - 1];
            double[] r = new double[n];
            double[] A = new double[n];
            double[] B = new double[n];
            double[] C = new double[n];

            double dx1, dx2, dy1, dy2;

            dx1 = x[1] - x[0];
            C[0] = 1.0f / dx1;
            B[0] = 2.0f * C[0];
            r[0] = 3 * (y[1] - y[0]) / (dx1 * dx1);

            for (int i = 1; i < n - 1; i++)
            {
                dx1 = x[i] - x[i - 1];
                dx2 = x[i + 1] - x[i];
                A[i] = 1.0f / dx1;
                C[i] = 1.0f / dx2;
                B[i] = 2.0f * (A[i] + C[i]);
                dy1 = y[i] - y[i - 1];
                dy2 = y[i + 1] - y[i];
                r[i] = 3 * (dy1 / (dx1 * dx1) + dy2 / (dx2 * dx2));
            }

            dx1 = x[n - 1] - x[n - 2];
            dy1 = y[n - 1] - y[n - 2];
            A[n - 1] = 1.0f / dx1;
            B[n - 1] = 2.0f * A[n - 1];
            r[n - 1] = 3 * (dy1 / (dx1 * dx1));

            double[] cPrime = new double[n];
            cPrime[0] = C[0] / B[0];
            for (int i = 1; i < n; i++)
                cPrime[i] = C[i] / (B[i] - cPrime[i - 1] * A[i]);

            double[] dPrime = new double[n];
            dPrime[0] = r[0] / B[0];
            for (int i = 1; i < n; i++)
                dPrime[i] = (r[i] - dPrime[i - 1] * A[i]) / (B[i] - cPrime[i - 1] * A[i]);

            double[] k = new double[n];
            k[n - 1] = dPrime[n - 1];
            for (int i = n - 2; i >= 0; i--)
                k[i] = dPrime[i] - cPrime[i] * k[i + 1];

            for (int i = 1; i < n; i++)
            {
                dx1 = x[i] - x[i - 1];
                dy1 = y[i] - y[i - 1];
                a[i - 1] = k[i - 1] * dx1 - dy1;
                b[i - 1] = -k[i] * dx1 + dy1;
            }

            return (a, b);
        }
    }
}
