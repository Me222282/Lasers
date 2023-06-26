using System;
using Zene.Windowing;
using Zene.Graphics;
using Zene.Structs;
using System.IO;
using Zene.GUI;

namespace Lasers
{
    class Program : Element
    {   
        public Program()
        {
            Graphics = new LocalGraphics(this, OnRender);
            Layout = new Layout(0d, 0d, 2d, 2d);
            //Children = new ElementList(this);
            
            _animator = new Animator();
            
            _context = new LineDC();
            _engine = new LightingEngine();
            _ray = new DisperseRays()
            {
                Colour = ColourF3.Yellow,
                Distance = 2d,
                Range = Radian.Degrees(5d),
                RayCount = 100
            };
            _engine.LightSources.Add(_ray);
            GenerateObjects();
            
            _spin = new AnimatorData<Radian>((v) =>
            {
                _ray.Direction = (Math.Cos(v), Math.Sin(v));
            }, 20d, 0d, Radian.Full)
            {
                Looping = true
            };
            
            _circleAnimation = new AnimatorData<double>((v) =>
            {
                _hoverCircleSize = v;
            }, 0.08, 0d, _selectRange, Animator.Exponential);
        }
        
        //public override ElementList Children { get; }
        public override GraphicsManager Graphics { get; }
        private Animator _animator { get; }
        private AnimatorData<Radian> _spin;
        
        static void Main(string[] args)
        {
            Core.Init();
            
            GUIWindow p = new GUIWindow(800, 500, "LIGHT");
            p.AddChild(new Program());
            p.RootElement.ShiftFocusRight();
            //p.Run();
            p.RunMultithread();
            
            Core.Terminate();
        }
        
        private LightingEngine _engine;
        private LineDC _context;
        
        //private RaySource _ray;
        private DisperseRays _ray;
        private const int _objCOunt = 2;
        private double _multiplier = 1d;
        
        private double _hoverCircleSize = 10d;
        private AnimatorData<double> _circleAnimation;
        
        protected override void OnUpdate(EventArgs e)
        {
            base.OnUpdate(e);
            
            _animator.Invoke();
            
            _multiplier = _renderScale.X / (Size.X * 0.5d);
            _engine.RenderLights(_context);
        }
        private void OnRender(object sender, RenderArgs e)
        {
            ColourF clear = ColourF.Black;
            if (!_engine.ReflectiveBounds)
            {
                clear = new ColourF(0.1f, 0.1f, 0.1f);
            }
            e.Context.Framebuffer.Clear(clear);
            
            _context.Framebuffer = e.Context.Framebuffer;
            _context.Model = Matrix.Identity;
            _context.View = Matrix.Identity;
            _context.Projection = Projection();
            if (_engine.ReflectiveBounds)
            {
                _context.DrawBox(_engine.Bounds, new ColourF(0.1f, 0.1f, 0.1f));
            }
            
            for (int i = 0; i < _engine.Objects.Count; i++)
            {
                _engine.Objects[i].Render(_context);
            }
            
            if (_engine.ReflectiveBounds)
            {
                Box bounds = _engine.Bounds;
                
                _context.AddLine(
                    new LineData(
                        (bounds.Left, bounds.Top),
                        (bounds.Right, bounds.Top),
                        ColourF.White));
                _context.AddLine(
                    new LineData(
                        (bounds.Left, bounds.Bottom),
                        (bounds.Right, bounds.Bottom),
                        ColourF.White));
                _context.AddLine(
                    new LineData(
                        (bounds.Left, bounds.Top),
                        (bounds.Left, bounds.Bottom),
                        ColourF.White));
                _context.AddLine(
                    new LineData(
                        (bounds.Right, bounds.Top),
                        (bounds.Right, bounds.Bottom),
                        ColourF.White));
            }
            
            _context.RenderLines();
            _context.ClearLines();
            
            // UI element to help show selectable poitns
            if (_hover.Pass())
            {
                _context.DrawEllipse(new Box(_hover.Location, _hoverCircleSize * _multiplier), (ColourF)_hover.Colour);
            }
        }
        
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e[Keys.R])
            {
                Actions.Push(GenerateObjects);
                return;
            }
            if (e[Keys.B])
            {
                _engine.ReflectiveBounds = !_engine.ReflectiveBounds;
                return;
            }
            if (e[Keys.S])
            {
                if (_spin.Animating)
                {
                    _spin.Stop();
                    return;
                }
                
                _spin.Reset(_animator);
                return;
            }
        }
        
        private const double _selectRange = 10d;
        private QueryData _hover = QueryData.Fail;
        private LightObject _select;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            
            Vector2 mouse = e.Location * _multiplier;
            double range = _selectRange * _multiplier;
            
            for (int i = 0; i < _engine.Objects.Count; i++)
            {
                QueryData v = _engine.Objects[i].QueryMouseSelect(mouse, range);
                
                if (!v.Pass()) { continue; }
                
                _select = _engine.Objects[i];
                _hover = v;
                return;
            }
            
            _select = null;
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            
            _select = null;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            Vector2 mouse = e.Location * _multiplier;
            
            _hover.Shift = this[Mods.Shift];
            _hover.Control = this[Mods.Control];
            
            if (_select != null)
            {
                _select.MouseInteract(mouse, ref _hover);
                return;
            }
            
            QueryData oldHover = _hover;
            double range = _selectRange * _multiplier;
            for (int i = 0; i < _engine.Objects.Count; i++)
            {
                _hover = _engine.Objects[i].QueryMouseSelect(mouse, range);
                
                if (!_hover.Pass()) { continue; }
                
                if (oldHover != _hover)
                {
                    if (!_hover.Pass())
                    {
                        _circleAnimation.Stop();
                    }
                    
                    _circleAnimation.Reset(_animator);
                }
                
                break;
            }
            
            if (this[Keys.Space])
            {
                Vector2 dir = (mouse - _ray.Location).Normalised();
                
                _ray.Direction = dir;
                return;
            }
        }
        
        private Vector2 _renderScale = Vector2.One;
        private IMatrix Projection()
        {
            double winWidth = Size.X;
            double winHeight = Size.Y;

            double w;
            double h;
            Box bounds = _engine.Bounds;
            
            if ((winHeight * bounds.Width) > (winWidth * bounds.Height))
            {
                w = bounds.Width;
                h = (winHeight / winWidth) * bounds.Width;
            }
            else // Width is bigger
            {
                h = bounds.Height;
                w = (winWidth / (double)winHeight) * bounds.Height;
            }
            
            _renderScale = (w * 0.5, h * 0.5);
            return Matrix4.CreateOrthographic(w, h, -1d, 1d);
        }
        
        private void GenerateObjects()
        {   
            Random r = new Random();
            _engine.Objects.Clear();
            
            for (int i = 0; i < _objCOunt; i++)
            {
                int type = r.Next(0, 2);
                
                if (type == 0)
                {
                    _engine.Objects.Add(new Mirror(InBoundsPos(r), InBoundsPos(r)));
                    continue;
                }
                
                _engine.Objects.Add(new GlassBlock(
                    InBoundsPos(r),
                    InBoundsPos(r),
                    InBoundsPos(r),
                    InBoundsPos(r),
                    r.NextDouble(0.5d, 2d)));
            }
            
            _ray.Location = InBoundsPos(r);
            _ray.Direction = r.NextVector2(2d) - 1d;
        }
        private Vector2 InBoundsPos(Random r)
        {
            Box b = _engine.Bounds;
            return r.NextVector2(b.Left, b.Right, b.Bottom, b.Top);
        }
    }
}
