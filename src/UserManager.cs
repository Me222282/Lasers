using System;
using Zene.Graphics;
using Zene.GUI;
using Zene.Structs;
using Zene.Windowing;

namespace Lasers
{
    public class UserManager : IRenderable<double>
    {
        public UserManager(Element handle, LightingEngine engine, Animator animator)
        {
            _handle = handle;
            _engine = engine;
            _animator = animator;
            
            handle.Scroll += OnScroll;
            handle.KeyDown += OnKeyDown;
            handle.KeyUp += OnKeyUp;
            
            _circleAnimation = new AnimatorData<double>((v) =>
            {
                _hoverCircleSize = v;
            }, 0.08, 0d, _selectRange, Animator.Exponential);
            
            // TEMP
            _ray = (DisperseRays)_engine.LightSources[0];
        }
        
        private readonly Element _handle;
        private readonly LightingEngine _engine;
        private readonly Animator _animator;
        
        private DisperseRays _ray;
        
        private const double _selectRange = 10d;
        private QueryData _objHover = QueryData.Fail;
        private bool _objPointSelect = false;
        private LightObject _moveObject = null;
        private bool _isMoveObject = false;
        private double _multiplier = 1d;
        private Vector2 _mouseOld;
        private Vector2 _m;
        
        private double _hoverCircleSize = 0d;
        private Vector2 _hoverPosition = Vector2.Zero;
        private AnimatorData<double> _circleAnimation;
        
        private Cursor CursorStyle
        {
            get => _handle.CursorStyle;
            set
            {
                if (value == _handle.CursorStyle) { return; }
                
                _handle.CursorStyle = value;
            }
        }
        
        public void OnRender(IDrawingContext context, double multiplier)
        {
            _multiplier = multiplier;
            
            // UI element to help show selectable poitns
            //if (_hover.Pass())
            if (_hoverCircleSize > 0d)
            {
                //context.DrawEllipse(new Box(_hoverPosition, _hoverCircleSize * _multiplier), ColourF.LightCyan);
                context.DrawRing(new Box(_hoverPosition, _hoverCircleSize * _multiplier), 2d * _multiplier, ColourF.PowderBlue);
            }
        }
        
        public void OnMouseDown(Vector2 mouse, MouseButton button)
        {
            if (button == MouseButton.Left)
            {
                if (_objHover.Source != null)
                {
                    _objPointSelect = true;
                    return;
                }
                
                if (_moveObject != null)
                {
                    _isMoveObject = true;
                }
                return;
            }
        }
        public void OnMouseUp(MouseButton button)
        {
            if (button == MouseButton.Left)
            {
                _objPointSelect = false;
                _isMoveObject = false;
                return;
            }
        }
        public void OnMouseMove(Vector2 mouse)
        {
            _m = mouse;
            
            // Panning
            if (_handle[MouseButton.Middle])
            {
                _handle.ViewPan += _handle.MouseLocation - _mouseOld;
                _mouseOld = _handle.MouseLocation;
            }
            else
            {
                _mouseOld = _handle.MouseLocation;
            }
            
            // Ray direction tracking
            if (_handle[Keys.Space])
            {
                Vector2 dir = (mouse - _ray.Location).Normalised();
                
                _ray.Direction = dir;
            }
            
            // Selecting obj hover point
            if (_objPointSelect)
            {
                _objHover.Shift = _handle[Mods.Shift];
                _objHover.Control = _handle[Mods.Control];
                Vector2 hp = _objHover.Source.MouseInteract(mouse, _objHover);
                _hoverPosition = hp;
                return;
            }
            
            if (_isMoveObject)
            {
                _moveObject.SetObjPos(mouse);
                return;
            }
            
            FindHovers(mouse);
        }
        
        private void OnScroll(object s, ScrollEventArgs e)
        {
            if (_objPointSelect)
            {
                _objHover.Scroll += e.DeltaY;
                _objHover.Shift = _handle[Mods.Shift];
                _objHover.Control = _handle[Mods.Control];
                _objHover.Source.MouseInteract(_hoverPosition, _objHover);
                return;
            }
            
            if (_handle[Mods.Control])
            {
                _ray.Distance += e.DeltaY * _ray.Distance * 0.1;
                return;
            }
            
            //ViewScale += e.DeltaY * 0.1;
            ZoomOnScreenPoint(_handle.MouseLocation, e.DeltaY);
        }
        private void ZoomOnScreenPoint(Vector2 point, double zoom)
        {
            double newZoom = _handle.ViewScale + (zoom * 0.1 * _handle.ViewScale);

            if (newZoom < 0) { return; }

            double oldZoom = _handle.ViewScale;
            _handle.ViewScale = newZoom;

            Vector2 pointRelOld = (point - _handle.ViewPan) / oldZoom;
            Vector2 pointRelNew = (point - _handle.ViewPan) / newZoom;

            _handle.ViewPan += (pointRelNew - pointRelOld) * newZoom;
        }
        
        private bool _alt = false;
        private void OnKeyDown(object s, KeyEventArgs e)
        {
            if (e[Keys.LeftAlt] || e[Keys.RightAlt])
            {
                _objHover = QueryData.Fail;
                if (_hoverCircleSize > 0d &&
                    (!_circleAnimation.Animating ||
                    !_circleAnimation.Reversed))
                {
                    _circleAnimation.Reversed = true;
                    _circleAnimation.Reset(_animator);
                }
                _alt = true;
                FindHovers(_m);
            }
        }
        private void OnKeyUp(object s, KeyEventArgs e)
        {
            if (e[Keys.LeftAlt] || e[Keys.RightAlt])
            {
                _alt = false;
                FindHovers(_m);
            }
        }
        
        private void SelectObject(Vector2 mouse)
        {
            double range = _selectRange * _multiplier;
            
            for (int i = 0; i < _engine.Objects.Count; i++)
            {
                QueryData v = _engine.Objects[i].QueryMousePos(mouse, range);
                
                if (!v.Pass()) { continue; }
                
                _objHover = v;
                _hoverPosition = v.Location;
                return;
            }
            
            _objHover = QueryData.Fail;
        }
        private void FindHovers(Vector2 mouse)
        {
            if (_alt)
            {
                goto SkipPointHovers;
            }
            
            // Find obj hover point
            QueryData oldHover = _objHover;
            SelectObject(mouse);
            if (oldHover != _objHover)
            {
                ManageNewHover(oldHover);
            }
            // Found hover object
            if (_objHover.Pass()) { return; }
            
        SkipPointHovers:
            HoverObject(mouse);
            
            // Found boundary hover point
            BoundaryHover(mouse);
        }
        private void ManageNewHover(QueryData oldHover)
        {
            // New hover point - apply animation
            if (_objHover.Pass())
            {
                _circleAnimation.Reversed = false;
                _circleAnimation.Reset(_animator);
                CursorStyle = Cursor.Hand;
                return;
            }
            
            CursorStyle = Cursor.Arrow;
            
            // Removed hover point - apply reversed animation
            if (oldHover.Pass())
            {
                _circleAnimation.Reversed = true;
                _circleAnimation.Reset(_animator);
            }
        }
        private void BoundaryHover(Vector2 mouse)
        {
            
        }
        private void HoverObject(Vector2 mouse)
        {
            for (int i = 0; i < _engine.Objects.Count; i++)
            {
                if (!_engine.Objects[i].MouseOverObject(mouse, _selectRange * _multiplier)) { continue; }
                
                _moveObject = _engine.Objects[i];
                CursorStyle = Cursor.ResizeAll;
                return;
            }
            
            CursorStyle = Cursor.Arrow;
            _moveObject = null;
        }
    }
}