using System;
using Zene.Windowing;
using Zene.Graphics;
using Zene.Structs;
using System.Collections.Generic;
using System.IO;

namespace Lasers
{
    class Program : Window
    {
        static void Main(string[] args)
        {
            Core.Init();
            
            Window p = new Program(800, 500, "LIGHT");
            
            p.Run();
            
            Core.Terminate();
        }
        
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
        private enum MouseMode
        {
            Select,
            AddLine,
            MoveSource
        }
        
        public unsafe Program(int width, int height, string title)
            : base(width, height, title)
        {
            _textRenderer = new NewTextRenderer()
            {
                Projection = Matrix4.CreateOrthographic(width, height, 0, -1)
            };
            _font = new DFFont2();
            _shader = new BasicShader();
            
            _lineBuffer = new ArrayBuffer<LinePoint>(1, BufferUsage.DrawFrequent);
            _lineBuffer.InitData(_lines.Count);
            _lineArray = new DrawingArray();
            // Vertices
            _lineArray.AddBuffer(_lineBuffer, 0, 0, DataType.Double, AttributeSize.D2);
            // Colour
            _lineArray.AddBuffer(_lineBuffer, (uint)BasicShader.Location.ColourAttribute, sizeof(Vector2), sizeof(LinePoint), DataType.Float, AttributeSize.D4);
            
            if (File.Exists(_saveFile))
            {
                FileStream fs = new FileStream(_saveFile, FileMode.Open);
                LoadState(fs);
                fs.Close();
            }
            
            // Blending
            State.Blending = true;
            Zene.Graphics.Base.GL.BlendFunc(Zene.Graphics.Base.GLEnum.SrcAlpha, Zene.Graphics.Base.GLEnum.OneMinusSrcAlpha);
        }
        
        private readonly NewFont _font;
        private readonly NewTextRenderer _textRenderer;
        private readonly BasicShader _shader;
        
        private readonly ArrayBuffer<LinePoint> _lineBuffer;
        private readonly DrawingArray _lineArray;
        
        private readonly List<LinePoint> _walls = new List<LinePoint>();
        private readonly List<LinePoint> _lines = new List<LinePoint>();
        
        protected override void OnUpdate(EventArgs e)
        {
            base.OnUpdate(e);
            
            Framebuffer.Clear(new ColourF(0.1f, 0.1f, 0.1f));
            _shader.Bind();
            
            _lines.Clear();
            CurrentWall();
            _lines.AddRange(_walls);
            CalculateRay();
            
            if (_lines.Count > 0)
            {
                _lineBuffer.SetData(_lines.ToArray());
            }
            else
            {
                _lineBuffer.InitData(0);
            }
            
            Vector2 offset = _offset;
            if (_move)
            {
                offset += MouseChange();
            }
            _shader.Matrix2 = Matrix4.CreateTranslation((Vector3)offset) * Matrix4.CreateScale(_zoom);
            
            _shader.ColourSource = ColourSource.AttributeColour;
            _lineArray.Draw<LinePoint>(DrawMode.Lines, 0);
            
            _textRenderer.Model = Matrix4.CreateScale(10) * Matrix4.CreateTranslation(0, (Height * 0.5) - 10);
            _textRenderer.DrawLeftBound($"Bounces: {_bounceCount}", _font, 0, 0);
        }
        
        private Vector2 MousePos
        {
            get => (((2d / (Vector2)Size) * MouseLocation) - 1d) * (1d, -1d);
        }
        private Vector2 MouseLocal()
        {
            return (MousePos / _zoom) - _offset;
        }
        
        private readonly ColourF _wallColour = new ColourF(1f, 1f, 1f);
        
        private bool _drawWallPoint = false;
        private bool _moveWall = false;
        private int _currentWallIndex = -1;
        private Vector2 _moveOffset = 0d;
        private Vector2 _moveOffsetB = 0d;
        private void CurrentWall()
        {
            if (_drawWallPoint)
            {
                _walls[_currentWallIndex] = new LinePoint(
                    MouseLocal() + _moveOffset,
                    _wallColour
                );
                return;
            }
            
            if (!_moveWall) { return; }
            
            _walls[_currentWallIndex] = new LinePoint(
                MouseLocal() + _moveOffset,
                _wallColour
            );
            _walls[_currentWallIndex + 1] = new LinePoint(
                MouseLocal() + _moveOffsetB,
                _wallColour
            );
        }
        
        private const double _selectDist = 0.01 * 0.01;
        private void SelectWall()
        {
            _drawWallPoint = true;
            
            Vector2 mouse = MouseLocal();
            double currentD = 1d;
            
            for (int i = 0; i < _walls.Count; i++)
            {
                Vector2 pos = _walls[i].Location;
                
                double d = pos.SquaredDistance(mouse);
                if (((d * _zoom * _zoom) < _selectDist) && (d < currentD))
                {
                    currentD = d;
                    _currentWallIndex = i;
                    _moveOffset = pos - mouse;
                }
            }
            // Point found
            if (_currentWallIndex >= 0) { return; }
            
            _drawWallPoint = false;
            _moveWall = true;
            currentD = 1d;
            
            for (int i = 0; i < _walls.Count; i += 2)
            {
                Segment2 seg = new Segment2(
                    _walls[i].Location,
                    _walls[i + 1].Location
                );
                
                double d = mouse.SquaredDistance(seg);
                if (((d * _zoom * _zoom) < _selectDist) && (d < currentD))
                {
                    currentD = d;
                    _currentWallIndex = i;
                    _moveOffset = seg.A - mouse;
                    _moveOffsetB = seg.B - mouse;
                }
            }
            if (_currentWallIndex < 0)
            {
                _moveWall = false;
                return;
            }
        }
        
        private double _zoom = 1d;
        private Vector2 _offset = Vector2.Zero;
        private Vector2 _mouseOld;
        private bool _move;
        
        private MouseMode _mouseMode = MouseMode.Select;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            
            if (e.Button == MouseButton.Left)
            {
                if (_drawWallPoint)
                {
                    _drawWallPoint = false;
                    _currentWallIndex = -1;
                    return;
                }
                
                switch (_mouseMode)
                {
                    case MouseMode.Select:
                        SelectWall();
                        // No wall selected
                        if (!(_drawWallPoint || _moveWall))
                        {
                            _mouseOld = MousePos;
                            _move = true;
                        }
                        return;
                        
                    case MouseMode.AddLine:
                        _drawWallPoint = true;
                        _walls.Add(new LinePoint(
                            MouseLocal(),
                            _wallColour
                        ));
                        _walls.Add(new LinePoint(
                            MouseLocal(),
                            _wallColour
                        ));
                        _currentWallIndex = _walls.Count - 1;
                        return;
                        
                    case MouseMode.MoveSource:
                        _source = MouseLocal();
                        return;
                }
                
                return;
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            
            if (e.Button == MouseButton.Left)
            {
                if (_move)
                {
                    _move = false;
                    _offset += MouseChange();
                }
                
                _drawWallPoint = false;
                _moveWall = false;
                _currentWallIndex = -1;
                _moveOffset = 0d;
                return;
            }
        }
        
        private Vector2 MouseChange()
        {
            Vector2 value = (MousePos - _mouseOld) / _zoom;

            //value.Y = -value.Y;

            return value;
        }
        
        //private const double _minDist = 1d / 1_000_000d;
        protected override void OnScroll(ScrollEventArgs e)
        {
            base.OnScroll(e);
            
            if (this[Mods.Alt])
            {
                _distance += _distance * e.DeltaY * 0.1;
                
                if (_distance < 0.01)
                {
                    _distance = 0.01;
                }
                return;
            }
            if (this[Mods.Control])
            {
                if (_move)
                {
                    _offset += MouseChange();
                    _mouseOld = MousePos;
                }

                double newZoom = _zoom + (e.DeltaY * 0.1 * _zoom);

                if (newZoom < 0) { return; }

                double oldZoom = _zoom;
                _zoom = newZoom;

                // Zoom in on mouse

                Vector2 mouse = MousePos;

                Vector2 mouseRelOld = (mouse / oldZoom) - _offset;
                Vector2 mouseRelNew = (mouse / _zoom) - _offset;

                _offset += mouseRelNew - mouseRelOld;
                return;
            }
            if (this[Mods.Shift])
            {
                _offset.X += e.DeltaY * 0.1 / _zoom;
                return;
            }
            else
            {
                _offset.Y -= e.DeltaY * 0.1 / _zoom;
                return;
            }
        }
        
        private int _bounceCount = 0;
        private bool _followMouse = true;
        
        private bool _drawLight = true;
        private Vector2 _source = (0d, 0d);
        private Vector2 _rayDir = ((Vector2)(0.2d, 0.5d)).Normalised();
        private double _distance = 10;
        private void CalculateRay()
        {
            _bounceCount = 0;
            
            if (!_drawLight) { return; }
            
            if (_followMouse)
            {
                _rayDir = (MouseLocal() - _source).Normalised();
            }
            
            Line2 ray = new Line2(_rayDir, _source);
            
            List<LinePoint> points = new List<LinePoint>()
            {
                new LinePoint(_source, new ColourF(1f, 1f, 0f))
            };
            
            double distTravel = 0;
            Vector2 currentPoint = _source;
            int lastReflect = -1;
            
            while (true)
            {   
                if (distTravel >= _distance) { break; }
                if (double.IsInfinity(distTravel) ||
                    double.IsNaN(distTravel)) { break; }
                
                Segment2 segTest = new Segment2(ray, _distance - distTravel);
                
                Vector2 nextPoint = 2;
                
                Segment2 closestWall = new Segment2();
                Vector2 wallIntsect = 0;
                double wallDist = 10;
                int lastReflectTemp = lastReflect;
                for (int i = 0; i < _walls.Count; i += 2)
                {
                    if (lastReflect == i) { continue; }
                    
                    Segment2 wall = new Segment2(_walls[i].Location, _walls[i + 1].Location);
                    
                    if (segTest.Intersects(wall, out Vector2 intersection))
                    {
                        double dist = currentPoint.SquaredDistance(intersection);
                        // Too far away
                        if (dist >= wallDist) { continue; }
                        
                        closestWall = wall;
                        wallDist = dist;
                        wallIntsect = intersection;
                        
                        lastReflectTemp = i;
                    }
                }
                
                lastReflect = lastReflectTemp;
                
                if (wallDist != 10)
                {
                    Vector2 reflection = Reflect(ray.Direction, closestWall.Change);
                    
                    ray = new Line2(
                        reflection,
                        wallIntsect
                    );
                    nextPoint = wallIntsect;
                }
                else
                {
                    if (segTest.B.X <= 1d &&
                    segTest.B.X >= -1d &&
                    segTest.B.Y <= 1d &&
                    segTest.B.Y >= -1d)
                    {
                        points.Add(new LinePoint(
                            segTest.B,
                            new ColourF(1f, 1f, 0f, 0f)
                        ));
                        break;
                    }
                    
                    ray.Direction = ray.Direction.Normalised();
                    
                    WallBounce(ref ray);
                    nextPoint = ray.Location;
                    
                    lastReflect = -1;
                }
                
                double distAdd = (currentPoint - nextPoint).Length;
                if (distAdd > 0)
                {
                    distTravel += distAdd;
                }
                
                float travPecent = (float)(distTravel / _distance);
                LinePoint p = new LinePoint(
                    nextPoint,
                    new ColourF(1f, 1f, 0f, 1f - travPecent)
                );
                // End this line
                points.Add(p);
                // Start next line
                points.Add(p);
                
                currentPoint = nextPoint;
                _bounceCount++;
            }
            
            if (_temp)
            {
                _lines.AddRange(points);
            }
            //_lines.AddRange(CurveLines(points));
            _lines.AddRange(Cubic.InterpolateXY(points, 5));
        }
        private Vector2 Reflect(Vector2 dir, Vector2 line)
        {
            Radian lineA = Math.Atan2(line.Y, line.X);
            Radian dirA = Math.Atan2(dir.Y, dir.X);
            
            Radian newA = (lineA * 2d) - dirA;
            
            return (
                Math.Cos(newA),
                Math.Sin(newA)
            );
        }
        private void WallBounce(ref Line2 ray)
        {
            double xp = 0;
            if (ray.Direction.X > 0)
            {
                xp = 1d;
            }
            else
            {
                xp = -1d;
            }
            double yTest = ray.GetY(xp);
            
            if (yTest <= 1d &&
                yTest >= -1d)
            {
                ray = new Line2(
                    (
                        -ray.Direction.X,
                        ray.Direction.Y
                    ),
                    (xp, yTest)
                );
                return;
            }
            
            double yp = 0;
            if (ray.Direction.Y > 0)
            {
                yp = 1d;
            }
            else
            {
                yp = -1d;
            }
            
            ray = new Line2(
                (
                    ray.Direction.X,
                    -ray.Direction.Y
                ),
                (ray.GetX(yp), yp)
            );
        }
        
        private bool _temp = false;
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e[Keys.Escape])
            {
                Close();
                return;
            }
            if (e[Keys.L])
            {
                //_drawLight = !_drawLight;
                _followMouse = !_followMouse;
                return;
            }
            if (e[Keys.C])
            {
                _walls.Clear();
                return;
            }
            if (e[Keys.Space])
            {
                _mouseMode = MouseMode.Select;
                CursorStyle = Cursor.Default;
                return;
            }
            if (e[Keys.A])
            {
                _mouseMode = MouseMode.AddLine;
                CursorStyle = Cursor.CrossHair;
                return;
            }
            if (e[Keys.S])
            {
                _mouseMode = MouseMode.MoveSource;
                CursorStyle = Cursor.ResizeAll;
                return;
            }
            if (e[Keys.T])
            {
                _temp = !_temp;
                return;
            }
        }
        protected override void OnSizeChange(SizeChangeEventArgs e)
        {
            base.OnSizeChange(e);
            
            _textRenderer.Projection = Matrix4.CreateOrthographic(e.Width, e.Height, 0, -1);
        }

        protected override void OnClosing(EventArgs e)
        {
            base.OnClosing(e);
            
            try
            {
                SaveState();
            }
            catch (Exception) { }
        }
        private const string _saveFile = "lightPathSaveData";
        private void SaveState()
        {
            FileStream fs = new FileStream(_saveFile, FileMode.Create);
            
            fs.Write(_followMouse);
            fs.Write(_drawLight);
            fs.Write(_source);
            fs.Write(_rayDir);
            fs.Write(_distance);
            fs.Write(_zoom);
            fs.Write(_offset);
            fs.Write(_walls.ToArray());
            
            fs.Close();
        }
        private void LoadState(Stream s)
        {
            try
            {
                _followMouse = s.Read<bool>();
                _drawLight = s.Read<bool>();
                _source = s.Read<Vector2>();
                _rayDir = s.Read<Vector2>();
                _distance = s.Read<double>();
                _zoom = s.Read<double>();
                _offset = s.Read<Vector2>();
                _walls.AddRange(
                    s.ReadArray<LinePoint>()
                );
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid or corrupt save file.");
                
                // Reset values
                _followMouse = true;
                _drawLight = true;
                _source = 0d;
                _rayDir = (0.2d, 0.5d);
                _distance = 10d;
                _zoom = 1d;
                _offset = 0d;
                _walls.Clear();
            }
        }
        
        private LinePoint[] CurveLines(List<LinePoint> p)
        {
            if (p.Count % 2 != 0)
            {
                return p.ToArray();
            }
            
            int lc = p.Count * 2;
            
            LinePoint[] newLines = new LinePoint[lc];
            
            double addition = 0.5 / lc;
            
            newLines[0] = new LinePoint(
                Lerp(p, 0d),
                p[0].Colour
            );
            
            double pi = 0.5;
            for (int i = 1; i < (lc - 1); i += 2)
            {
                Vector2 v = Lerp(p, addition * (i + 1));
                
                newLines[i] = new LinePoint(
                    v, ReadLerpValue(p, pi)
                );
                newLines[i + 1] = new LinePoint(
                    v, ReadLerpValue(p, pi)
                );
                
                pi += 0.5;
            }
            
            newLines[lc - 1] = new LinePoint(
                Lerp(p, 1d),
                p[p.Count - 1].Colour
            );
            
            return newLines;
        }
        
        private Vector2 Lerp(List<LinePoint> v, double b)
        {   
            Vector2[] lastLerps = new Vector2[v.Count / 2];
            
            int lc = 0;
            for (int i = 0; i < v.Count; i += 2)
            {
                Vector2 c = v[i].Location.Lerp(v[i + 1].Location, b);
                
                Vector2 last = c;
                
                for (int l = 0; l < lc; l++)
                {
                    Vector2 old = last;
                    
                    last = lastLerps[l].Lerp(last, b);
                    
                    lastLerps[l] = old;
                }
                
                lastLerps[lc] = last;
                
                lc++;
            }
            
            return lastLerps[lastLerps.Length - 1];
        }
        
        private ColourF ReadLerpValue(List<LinePoint> l, double i)
        {
            double a = l[(int)i].Colour.A;
            float b = l[((int)i) + 1].Colour.A;
            
            //return Lerp(a, b, (float)i - ((int)i));
            return new ColourF(
                0f, 0.5f, 0.8f,
                (float)a.Lerp(b, i - ((int)i))
            );
        }
        private ColourF Lerp(ColourF a, ColourF b, float l)
        {
            return new ColourF(
                a.R + ((b.R - a.R) * l),
                a.G + ((b.B - a.B) * l),
                a.B + ((b.G - a.G) * l),
                a.A + ((b.A - a.A) * l)
            );
        }
    }
}
