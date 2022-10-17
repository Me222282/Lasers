using System;
using Zene.Windowing;
using Zene.Graphics;
using Zene.Structs;
using System.Collections.Generic;
using System.IO;

namespace Lasers
{
    class NewProgram : Window
    {
        private struct LinePoint
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
        
        public unsafe NewProgram(int width, int height, string title)
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
            
            _shader.ColourSource = ColourSource.AttributeColour;
            _lineArray.Draw<LinePoint>(DrawMode.Lines, 0);
            
            _textRenderer.Model = Matrix4.CreateScale(10) * Matrix4.CreateTranslation(0, (Height * 0.5) - 10);
            _textRenderer.DrawLeftBound($"Bounces: {_bounceCount}", _font, 0, 0);
        }
        
        private Vector2 MouseLocal()
        {
            return (((2d / (Vector2)Size) * MouseLocation) - 1d) * (1d, -1d);
        }
        
        private readonly ColourF _wallColour = new ColourF(1f, 1f, 1f);
        
        private bool _drawWall = false;
        private int _currentWallIndex = -1;
        private void CurrentWall()
        {
            if (!_drawWall) { return; }
            
            _walls[_currentWallIndex] = new LinePoint(
                MouseLocal(),
                _wallColour
            );
        }
        
        private const double _selectDist = 0.05 * 0.05;
        private void SelectWall()
        {
            Vector2 mouse = MouseLocal();
            
            double currentD = 1d;
            
            for (int i = 0; i < _walls.Count; i++)
            {
                Vector2 pos = _walls[i].Location;
                
                double d = pos.SquaredDistance(mouse);
                if ((d < _selectDist) && (d < currentD))
                {
                    currentD = d;
                    _currentWallIndex = i;
                }
            }
            
            if (_currentWallIndex < 0)
            {
                _drawWall = false;
            }
        }
        
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            
            if (e.Button == MouseButton.Left)
            {
                if (_drawWall)
                {
                    _drawWall = false;
                    _currentWallIndex = -1;
                    return;
                }
                
                _drawWall = true;
                SelectWall();
                return;
            }
            if (e.Button == MouseButton.Right)
            {
                _source = MouseLocal();
                return;
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            
            if (e.Button == MouseButton.Left)
            {
                _drawWall = false;
                _currentWallIndex = -1;
                return;
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            if (_followMouse)
            {
                _rayDir = (MouseLocal() - _source).Normalised();
            }
        }
        
        //private const double _minDist = 1d / 1_000_000d;
        protected override void OnScroll(ScrollEventArgs e)
        {
            base.OnScroll(e);
            
            _distance += _distance * e.DeltaY * 0.1;
            
            if (_distance < 0.01)
            {
                _distance = 0.01;
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
            
            _lines.AddRange(points);
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
                _drawLight = !_drawLight;
                return;
            }
            if (e[Keys.C])
            {
                _walls.Clear();
                return;
            }
            if (e[Keys.Space])
            {
                _followMouse = !_followMouse;
                return;
            }
            if (e[Keys.A])
            {
                _drawWall = true;
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
            fs.Write(_walls.ToArray());
            
            fs.Close();
        }
        private void LoadState(Stream s)
        {
            _followMouse = s.Read<bool>();
            _drawLight = s.Read<bool>();
            _source = s.Read<Vector2>();
            _rayDir = s.Read<Vector2>();
            _distance = s.Read<double>();
            _walls.AddRange(
                s.ReadArray<LinePoint>()
            );
        }
    }
}
