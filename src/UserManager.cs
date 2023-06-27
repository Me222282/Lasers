using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zene.Graphics;
using Zene.GUI;
using Zene.Structs;
using Zene.Windowing;

namespace Lasers
{
    public class UserManager : IBasicRenderer<double>
    {
        public UserManager(Element handle, LightingEngine engine, Animator animator)
        {
            _handle = handle;
            _engine = engine;
            _animator = animator;
            
            handle.Scroll += OnSCroll;
            
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
        private QueryData _hover = QueryData.Fail;
        private LightObject _select;
        private double _multiplier = 1d;
        private Vector2 _mouseOld;
        
        private double _hoverCircleSize = 0d;
        private AnimatorData<double> _circleAnimation;
        
        public void OnRender(DrawManager context, double multiplier)
        {
            _multiplier = multiplier;
            
            // UI element to help show selectable poitns
            //if (_hover.Pass())
            if (_hoverCircleSize > 0d)
            {
                context.DrawEllipse(new Box(_hover.Location, _hoverCircleSize * _multiplier), (ColourF)_hover.Colour);
            }
        }
        
        public void OnMouseDown(Vector2 mouse, MouseButton button)
        {
            if (button == MouseButton.Left)
            {
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
                return;
            }
        }
        public void OnMouseUp(MouseButton button)
        {
            if (button == MouseButton.Left)
            {
                _select = null;
            }
        }
        public void OnMouseMove(Vector2 mouse)
        {
            _hover.Shift = _handle[Mods.Shift];
            _hover.Control = _handle[Mods.Control];
            
            if (_handle[MouseButton.Middle])
            {
                _handle.ViewPan += _handle.MouseLocation - _mouseOld;
                _mouseOld = _handle.MouseLocation;
            }
            else
            {
                _mouseOld = _handle.MouseLocation;
            }
            
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
                
                break;
            }
            if (oldHover != _hover)
            {
                _handle.CursorStyle = Cursor.Arrow;
                
                if (!_hover.Pass() && oldHover.Pass())
                {
                    _hover = new QueryData(1, oldHover.Location, oldHover.Colour, null);
                    _circleAnimation.Reversed = true;
                    _circleAnimation.Reset(_animator);
                }
                else if (!_hover.Pass())
                {
                    _hover = oldHover;
                }
                else
                {
                    _circleAnimation.Reversed = false;
                    _circleAnimation.Reset(_animator);
                    _handle.CursorStyle = Cursor.ResizeAll;
                }
            }
            
            if (_handle[Keys.Space])
            {
                Vector2 dir = (mouse - _ray.Location).Normalised();
                
                _ray.Direction = dir;
                return;
            }
        }
        
        private void OnSCroll(object s, ScrollEventArgs e)
        {
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
    }
}