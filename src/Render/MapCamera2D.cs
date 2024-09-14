using System.Linq;
using Godot;

namespace ModdingGodotTestingGame;

public partial class MapCamera2D : Camera2D
{
    [Export(PropertyHint.Range, "0.1, 10")] public float ZoomFactor { get; set; } = 1.25f;
    [Export(PropertyHint.Range, "0.01, 100")] public float ZoomMin { get; set; } = 0.1f;
    [Export(PropertyHint.Range, "0.01, 100")] public float ZoomMax { get; set; } = 10.0f;
    [Export] public bool ZoomRelative { get; set; } = true;
    [Export] public bool ZoomKeyboard { get; set; } = true;
    [Export(PropertyHint.Range, "0, 10000")] public float PanSpeed { get; set; } = 250.0f;
    [Export(PropertyHint.Range, "0, 1000")] public float PanMargin { get; set; } = 25.0f;
    [Export] public bool PanKeyboard { get; set; } = true;
    [Export] public bool Drag { get; set; } = true;
    [Export(PropertyHint.Range, "0, 1")] public float DragInertia { get; set; } = 0.1f;

    private Tween _tweenOffset;
    private Tween _tweenZoom;
    private Vector2 _panDirection = Vector2.Zero;
    private Vector2 _panDirectionMouse = Vector2.Zero;
    private bool _dragging;
    private Vector2 _dragMovement = Vector2.Zero;
    private Vector2 _targetZoom;

    public override void _Ready()
    {
        _targetZoom = Zoom;
        GetViewport().SizeChanged += ClampOffset;
    }

    public override void _Process(double dDelta)
    {
        var delta = (float) dDelta;

        if (_dragMovement == Vector2.Zero)
        {
            ClampOffset(_panDirection * PanSpeed * delta / Zoom);
        }
        else
        {
            _dragMovement *= Mathf.Pow(DragInertia, delta);
            ClampOffset(-_dragMovement / Zoom);

            if (_dragMovement.LengthSquared() < 0.01f)
            {
                SetProcess(false);
                SetPhysicsProcess(false);
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _Process(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMagnifyGesture magnifyGesture)
        {
            _ChangeZoom(1 + ((ZoomFactor > 1 ? ZoomFactor : 1 / ZoomFactor) - 1) * (magnifyGesture.Factor - 1) * 2.5f);
        }
        else if (@event is InputEventPanGesture panGesture)
        {
            _ChangeZoom(1 + (1 / ZoomFactor - 1) * panGesture.Delta.Y / 7.5f);
        }
        else if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.Pressed)
            {
                switch (mouseButton.ButtonIndex)
                {
                    case MouseButton.WheelUp:
                        _ChangeZoom(ZoomFactor);
                        break;
                    case MouseButton.WheelDown:
                        _ChangeZoom(1 / ZoomFactor);
                        break;
                    case MouseButton.Left:
                        if (Drag)
                        {
                            Input.SetDefaultCursorShape(Input.CursorShape.Drag);
                            _dragging = true;
                            _dragMovement = Vector2.Zero;
                            SetProcess(false);
                            SetPhysicsProcess(false);
                        }
                        break;
                }
            }
            else if (mouseButton.ButtonIndex == MouseButton.Left && _dragging)
            {
                Input.SetDefaultCursorShape();
                _dragging = false;

                if (_dragMovement != Vector2.Zero || _panDirection != Vector2.Zero)
                {
                    SetProcess(true);
                    SetPhysicsProcess(true);
                }
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion)
        {
            if (_panDirectionMouse != Vector2.Zero)
            {
                _panDirection -= _panDirectionMouse;
            }

            _panDirectionMouse = Vector2.Zero;

            if (_dragging)
            {
                _tweenOffset?.Kill();
                ClampOffset(-mouseMotion.Relative / Zoom);
                _dragMovement = mouseMotion.Relative;
            }
            else if (PanMargin > 0)
            {
                var cameraSize = GetViewportRect().Size;

                if (mouseMotion.Position.X < PanMargin)
                {
                    _panDirectionMouse.X -= 1;
                }

                if (mouseMotion.Position.X >= cameraSize.X - PanMargin)
                {
                    _panDirectionMouse.X += 1;
                }

                if (mouseMotion.Position.Y < PanMargin)
                {
                    _panDirectionMouse.Y -= 1;
                }

                if (mouseMotion.Position.Y >= cameraSize.Y - PanMargin)
                {
                    _panDirectionMouse.Y += 1;
                }

                if (_panDirectionMouse != Vector2.Zero)
                {
                    _panDirection += _panDirectionMouse;
                }
            }
        }
        else if (@event is InputEventKey keyEvent)
        {
            if (ZoomKeyboard && keyEvent.Pressed)
            {
                switch (keyEvent.Keycode)
                {
                    case Key.Minus:
                        _ChangeZoom(ZoomFactor < 1 ? ZoomFactor : 1 / ZoomFactor, false);
                        break;
                    case Key.Equal:
                        _ChangeZoom(ZoomFactor > 1 ? ZoomFactor : 1 / ZoomFactor, false);
                        break;
                }
            }

            if (PanKeyboard && !keyEvent.Echo)
            {
                switch (keyEvent.Keycode)
                {
                    case Key.Left:
                        _panDirection -= new Vector2(keyEvent.Pressed ? 1 : -1, 0);
                        break;
                    case Key.Right:
                        _panDirection += new Vector2(keyEvent.Pressed ? 1 : -1, 0);
                        break;
                    case Key.Up:
                        _panDirection -= new Vector2(0, keyEvent.Pressed ? 1 : -1);
                        break;
                    case Key.Down:
                        _panDirection += new Vector2(0, keyEvent.Pressed ? 1 : -1);
                        break;
                    case Key.Space:
                        if (keyEvent.Pressed)
                        {
                            _tweenOffset?.Kill();
                            Offset = Vector2.Zero;
                        }
                        break;
                }
            }
        }
    }

    private void ClampOffset()
    {
        ClampOffset(Vector2.Zero);
    }

    private void ClampOffset(Vector2 relative)
    {
        var cameraSize = GetViewportRect().Size / Zoom;
        var cameraRect = new Rect2(GlobalPosition + relative - cameraSize / 2, cameraSize);

        if (cameraRect.Position.X < LimitLeft)
        {
            _dragMovement.X = 0;
            relative.X += LimitLeft - cameraRect.Position.X;
            cameraRect.End += new Vector2(LimitLeft - cameraRect.Position.X,0);
        }

        if (cameraRect.End.X > LimitRight)
        {
            _dragMovement.X = 0;
            relative.X -= cameraRect.End.X - LimitRight;
        }

        if (cameraRect.End.Y > LimitBottom)
        {
            _dragMovement.Y = 0;
            relative.Y -= cameraRect.End.Y - LimitBottom;
            cameraRect.Position -= new Vector2(0,cameraRect.End.Y - LimitBottom);
        }

        if (cameraRect.Position.Y < LimitTop)
        {
            _dragMovement.Y = 0;
            relative.Y += LimitTop - cameraRect.Position.Y;
        }

        if (relative != Vector2.Zero)
        {
            Offset += relative;
        }
    }

    private void _ChangeZoom(float factor, bool withCursor = true)
    {
        if (factor < 1)
        {
            if (_targetZoom.X < ZoomMin || Mathf.IsEqualApprox(_targetZoom.X, ZoomMin))
                return;

            if (_targetZoom.Y < ZoomMin || Mathf.IsEqualApprox(_targetZoom.Y, ZoomMin))
                return;
        }
        else if (factor > 1)
        {
            if (_targetZoom.X > ZoomMax || Mathf.IsEqualApprox(_targetZoom.X, ZoomMax))
                return;

            if (_targetZoom.Y > ZoomMax || Mathf.IsEqualApprox(_targetZoom.Y, ZoomMax))
                return;
        }
        else
        {
            return;
        }

        _targetZoom *= factor;

        var clampedZoom = _targetZoom;
        clampedZoom *= ((float[]) [1, ZoomMin / _targetZoom.X, ZoomMin / _targetZoom.Y]).Max();
        clampedZoom *= ((float[]) [1, ZoomMax / _targetZoom.X, ZoomMax / _targetZoom.Y]).Min();

        if (PositionSmoothingEnabled && PositionSmoothingSpeed > 0)
        {
            if (ZoomRelative && withCursor && !IsProcessing() && !IsPhysicsProcessing())
            {
                var relativePosition = GetGlobalMousePosition() - GlobalPosition - Offset;
                var relative = relativePosition - relativePosition * Zoom / clampedZoom;

                _tweenOffset?.Kill();
                _tweenOffset = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out).SetProcessMode((Tween.TweenProcessMode)ProcessCallback);
                _tweenOffset.TweenProperty(this, "offset", Offset + relative, 2.5f / PositionSmoothingSpeed);
            }

            _tweenZoom?.Kill();
            _tweenZoom = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out).SetProcessMode((Tween.TweenProcessMode)ProcessCallback);
            _tweenZoom.TweenMethod(Callable.From((float value) => _SetZoomLevel(Vector2.One / value)), Vector2.One / Zoom, Vector2.One / clampedZoom, 2.5f / PositionSmoothingSpeed);
        }
        else
        {
            if (ZoomRelative && withCursor)
            {
                var relativePosition = GetGlobalMousePosition() - GlobalPosition - Offset;
                var relative = relativePosition - relativePosition * Zoom / clampedZoom;

                Zoom = clampedZoom;
                ClampOffset(relative);
            }
            else
            {
                _SetZoomLevel(clampedZoom);
            }
        }
    }

    private void _SetZoomLevel(Vector2 value)
    {
        Zoom = value;
        ClampOffset();
    }
}