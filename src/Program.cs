using System;
using Zene.Windowing;
using Zene.Graphics;
using Zene.Structs;

namespace Lasers
{
    class Program : Window
    {
        static void Main(string[] args)
        {
            Core.Init();
            
            Window p = new NewProgram(800, 500, "LIGHT");
            
            p.Run();
            
            Core.Terminate();
        }
        
        public Program(int width, int height, string title)
            : base(width, height, title)
        {
            _drawable = new DrawObject<float, byte>(stackalloc float[]
            {
                1f, 1f, 1f, 1f,
                -1f, 1f, 0f, 1f,
                -1f, -1f, 0f, 0f,
                1f, -1f, 1f, 0f
            }, stackalloc byte[] { 0, 1, 2, 2, 3, 0 }, 4, 0, AttributeSize.D2, BufferUsage.DrawFrequent);
            _drawable.AddAttribute((int)BasicShader.Location.TextureCoords, 2, AttributeSize.D2);
            
            _shader = new BasicShader();
            
            OnSizeChange(new SizeChangeEventArgs(width, height));
            
            _lightPath = Texture2D.Create(_lightPathData, WrapStyle.EdgeClamp, TextureSampling.Nearest, false);
            _walls = Texture2D.Create(_wallGraphicData, WrapStyle.EdgeClamp, TextureSampling.Nearest, false);
            
            // Blending
            State.Blending = true;
            Zene.Graphics.Base.GL.BlendFunc(Zene.Graphics.Base.GLEnum.SrcAlpha, Zene.Graphics.Base.GLEnum.OneMinusSrcAlpha);
        }
        
        private GLArray<ColourF> _lightPathData;
        private readonly Texture2D _lightPath;
        
        private GLArray<ColourF> _wallGraphicData;
        private GLArray<Vector3> _wallLineData;
        private readonly Texture2D _walls;
        
        private readonly DrawObject<float, byte> _drawable;
        private readonly BasicShader _shader;
        
        private int _levelOfDetail = 500;
        
        protected override void OnUpdate(EventArgs e)
        {
            base.OnUpdate(e);
            
            Framebuffer.Clear(new ColourF(0.1f, 0.1f, 0.1f));
            
            _shader.Bind();
            _shader.ColourSource = ColourSource.Texture;
            _shader.TextureSlot = 0;
            
            // Walls
            _walls.SetData(_wallGraphicData.Width, _wallGraphicData.Height, BaseFormat.Rgba, _wallGraphicData);
            _walls.Bind(0);
            _drawable.Draw();
            
            // Clear
            _lightPathData = new GLArray<ColourF>(_lightPathData.Width, _lightPathData.Height);
            
            DrawRay();
            DrawCurrentWall();
            _lightPathData[_source] = new ColourF(1f, 0f, 0f);
            _lightPath.SetData(_lightPathData.Width, _lightPathData.Height, BaseFormat.Rgba, _lightPathData);
            
            // Light Path
            _lightPath.Bind(0);
            _drawable.Draw();
        }
        
        private Vector2I ScaleSize(Vector2I windowSize)
        {
            if (windowSize.X > windowSize.Y)
            {
                return (
                    (int)((windowSize.X / (double)windowSize.Y) * _levelOfDetail),
                    _levelOfDetail
                );
            }
            
            return (
                _levelOfDetail,
                (int)((windowSize.Y / (double)windowSize.X) * _levelOfDetail)
            );
        }
        private Vector2I GridMousePos()
        {
            Vector2 ml = MouseLocation;
            
            return (
                (int)((_lightPathData.Width / (double)Width) * ml.X),
                (int)((_lightPathData.Height / (double)Height) * ml.Y)
            );
        }
        
        protected override void OnSizeChange(SizeChangeEventArgs e)
        {
            base.OnSizeChange(e);
            
            _lightPathData = new GLArray<ColourF>(ScaleSize(e.Size));
            _wallGraphicData = new GLArray<ColourF>(ScaleSize(e.Size));
            _wallLineData = new GLArray<Vector3>(ScaleSize(e.Size));
            _lineCount = 0;
        }
        
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e[Keys.Space])
            {
                _drawLight = !_drawLight;
                return;
            }
            if (e[Keys.C])
            {
                _wallGraphicData = new GLArray<ColourF>(ScaleSize(Size));
                _wallLineData = new GLArray<Vector3>(ScaleSize(Size));
                _lineCount = 0;
                return;
            }
            if (e[Keys.B])
            {
                if (_lightPath.MagFilter == TextureSampling.Nearest)
                {
                    _lightPath.MagFilter = TextureSampling.Blend;
                    _walls.MagFilter = TextureSampling.Blend;
                    return;
                }
                
                _lightPath.MagFilter = TextureSampling.Nearest;
                    _walls.MagFilter = TextureSampling.Nearest;
                return;
            }
        }
        protected override void OnScroll(ScrollEventArgs e)
        {
            base.OnScroll(e);
            
            if (this[Mods.Control])
            {
                _levelOfDetail += (int)(_levelOfDetail * e.DeltaY * 0.1);
                
                if (_levelOfDetail < 10)
                {
                    _levelOfDetail = 10;
                }
                
                OnSizeChange(new SizeChangeEventArgs(Size));
                return;
            }
            
            _distance += (int)(_distance * e.DeltaY * 0.1);
            
            if (_distance < 1)
            {
                _distance = 1;
            }
        }
        
        private bool _drawLight = true;
        private Vector2I _source = (50, 250);
        private Vector2 _direction = ((Vector2)(20d, 30d)).Normalised();
        private int _distance = 100;
        
        private void DrawRay()
        {   
            if (!_drawLight) { return; }
            
            Vector2 direction = (GridMousePos() - (Vector2)_source).Normalised();
            
            int distance = 0;
            
            double lastReflect = -1;
            
            double x = _source.X;
            double y = _source.Y;
            while (true)
            {
                x += direction.X;
                y += direction.Y;
                distance++;
                
                if (distance == _distance)
                {
                    return;
                }
                
                if (x < 0)
                {
                    x = 0;
                    direction.X = -direction.X;
                    lastReflect = -1;
                }
                else if (x >= _lightPathData.Width)
                {
                    x = _lightPathData.Width - 1;
                    direction.X = -direction.X;
                    lastReflect = -1;
                }
                if (y < 0)
                {
                    y = 0;
                    direction.Y = -direction.Y;
                    lastReflect = -1;
                }
                else if (y >= _lightPathData.Height)
                {
                    y = _lightPathData.Height - 1;
                    direction.Y = -direction.Y;
                    lastReflect = -1;
                }
                
                Vector3 area = GetWallArea((int)x, (int)y, direction);
                if ((area != 0) && (lastReflect != area.Z))
                {
                    lastReflect = area.Z;
                    direction = Reflect(direction, (Vector2)area);
                }
                
                float brightness = 1f - ((float)distance / _distance);
                WriteLight((int)x, (int)y, brightness);
            }
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
        private void WriteLight(int x, int y, float brightness)
        {
            if (_lightPathData[x, y].A >= brightness)
            {
                return;
            }
            
            _lightPathData[x, y] = new ColourF(1f, 1f, 0f, brightness);
        }
        private Vector3 GetWallArea(int x, int y, Vector2 dir)
        {
            Vector3 value;
            
            if ((value = _wallLineData[x, y]) != 0)
            {
                _lightPathData[x, y] = new ColourF(0f, 0f, 1f);
                return value;
            }
            if ((dir.X >= 0) && (value = _wallLineData[x + 1, y]) != 0)
            {
                _lightPathData[x + 1, y] = new ColourF(0f, 0f, 1f);
                return value;
            }
            if ((dir.Y >= 0) && (value = _wallLineData[x, y + 1]) != 0)
            {
                _lightPathData[x, y + 1] = new ColourF(0f, 0f, 1f);
                return value;
            }
            if ((dir.X <= 0) && (value = _wallLineData[x - 1, y]) != 0)
            {
                _lightPathData[x - 1, y] = new ColourF(0f, 0f, 1f);
                return value;
            }
            if ((dir.Y <= 0) && (value = _wallLineData[x, y - 1]) != 0)
            {
                _lightPathData[x, y - 1] = new ColourF(0f, 0f, 1f);
                return value;
            }/*
            if (((dir.X >= 0) || (dir.Y >= 0)) && (value = _wallLineData[x + 1, y + 1]) != 0)
            {
                return value;
            }
            if (((dir.X <= 0) || (dir.Y <= 0)) && (value = _wallLineData[x - 1, y - 1]) != 0)
            {
                return value;
            }
            if (((dir.X >= 0) || (dir.Y <= 0)) && (value = _wallLineData[x + 1, y - 1]) != 0)
            {
                return value;
            }
            if (((dir.X <= 0) || (dir.Y >= 0)) && (value = _wallLineData[x - 1, y + 1]) != 0)
            {
                return value;
            }*/
            
            return 0;
        }
        
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            
            if (e.Button == MouseButton.Left)
            {
                _source = GridMousePos();
                return;
            }
            if (e.Button == MouseButton.Right)
            {
                _drawingWall = true;
                _currentWallStart = GridMousePos();
                return;
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            
            if (e.Button == MouseButton.Right)
            {
                _drawingWall = false;
                PrintLine(_currentWallStart, _currentWallEnd, (x, y) =>
                {
                    _wallGraphicData[x, y] = new ColourF(1f, 1f, 1f);
                    
                    Vector2 box = _currentWallEnd - _currentWallStart;
                    _wallLineData[x, y] = (box.Normalised(), _lineCount);
                });
                _lineCount++;
                return;
            }
        }
        
        private int _lineCount = 0;
        private bool _drawingWall = false;
        private Vector2I _currentWallStart = 0;
        private Vector2I _currentWallEnd = 0;
        
        private void DrawCurrentWall()
        {
            if (!_drawingWall) { return; }
            
            _currentWallEnd = GridMousePos();
            
            PrintLine(_currentWallStart, _currentWallEnd, (x, y) =>
            {
                _lightPathData[x, y] = new ColourF(1f, 1f, 1f);
            });
        }
        
        private static void PrintLine(Vector2I startPos, Vector2I endPos, Action<int, int> write)
        {
            Vector2I size = endPos - startPos;
            
            // No line
            if (size.X == 0 && size.Y == 0) { return; }
            
            bool xBound = Math.Abs(size.X) > Math.Abs(size.Y);
            
            if (xBound)
            {
                Vector2I start = endPos.X < startPos.X ? endPos : startPos;
                
                int w = Math.Abs(size.X);
                
                double y = start.Y;
                double yIncrease = size.Y / (double)size.X;
                
                for (int x = 0; x < w; x++)
                {
                    write(x + start.X, (int)y);
                    
                    y += yIncrease;
                }
                
                return;
            }
            else
            {
                Vector2I start = endPos.Y < startPos.Y ? endPos : startPos;
                
                int h = Math.Abs(size.Y);
                
                double x = start.X;
                double xIncrease = size.X / (double)size.Y;
                
                for (int y = 0; y < h; y++)
                {
                    write((int)x, y + start.Y);
                    
                    x += xIncrease;
                }
                
                return;
            }
        }
    }
}
