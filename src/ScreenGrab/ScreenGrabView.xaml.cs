using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dapplo.Windows.User32;
using ScreenGrab.Extensions;
using ScreenGrab.Utilities;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace ScreenGrab;

/// <summary>
///     Extracted from the project <see href="https://github.com/TheJoeFin/Text-Grab"/>
/// </summary>
public partial class ScreenGrabView
{
    #region Constructors

    public ScreenGrabView(Action<Bitmap>? action, bool isPolyline = false)
    {
        InitializeComponent();
        _onImageCaptured = action;
        _isPolyline = isPolyline;
    }

    #endregion Constructors

    #region Properties

    public Action? OnGrabClose { get; set; }
    private DisplayInfo? CurrentScreen { get; set; }

    #endregion Properties

    #region Fields

    private Point _clickedPoint;
    private DpiScale? _dpiScale;
    private bool _isSelecting;
    private bool _isShiftDown;
    private readonly Border _selectBorder = new();
    private double _selectLeft;
    private double _selectTop;
    private Point _shiftPoint;
    private double _xShiftDelta;
    private double _yShiftDelta;

    private readonly Action<Bitmap>? _onImageCaptured;
    private readonly bool _isPolyline;

    #endregion Fields

    #region Window Events

    private void Window_Closed(object? sender, EventArgs e)
    {
        Close();

        GC.Collect();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Maximized;
        FullWindow.Rect = new Rect(0, 0, Width, Height);
        KeyDown += ScreenGrab_KeyDown;
        KeyUp += ScreenGrab_KeyUp;

        SetImageToBackground();

#if DEBUG
        Topmost = false;
#endif
        
        if (!_isPolyline) return;
        SetPolylineVisibility(true);
        (HorizontalLine.X1, VerticalLine.Y1, (HorizontalLine.X2, VerticalLine.Y2)) = (0, 0, this.GetWidthHeight());
    }

    private void Window_Unloaded(object sender, RoutedEventArgs e)
    {
        FreezeBgImage();
        BackgroundImage.UpdateLayout();
        CurrentScreen = null;
        _dpiScale = null;

        Loaded -= Window_Loaded;
        Unloaded -= Window_Unloaded;

        RegionClickCanvas.MouseDown -= RegionClickCanvas_MouseDown;
        RegionClickCanvas.MouseMove -= RegionClickCanvas_MouseMove;
        RegionClickCanvas.MouseUp -= RegionClickCanvas_MouseUp;

        KeyDown -= ScreenGrab_KeyDown;
        KeyUp -= ScreenGrab_KeyUp;
    }

    private void ScreenGrab_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                CloseAllFullscreenGrabs();
                break;
            case Key.F:
                FreezeUnfreezeAllScreenGrabs();
                break;
        }
    }

    private void ScreenGrab_KeyUp(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.LeftShift:
            case Key.RightShift:
                _isShiftDown = false;
                _clickedPoint = new Point(_clickedPoint.X + _xShiftDelta, _clickedPoint.Y + _yShiftDelta);
                break;
        }
    }

    private void SetImageToBackground()
    {
        FreezeBgImage();
        BackgroundImage.Source = this.GetWindowBoundsImage();
        BackgroundBrush.Opacity = 0.2;
    }

    private async void FreezeUnfreeze()
    {
        if (BackgroundImage.Source == null)
        {
            if (_isPolyline)
            {
                SetPolylineVisibility(false);
            }
            BackgroundBrush.Opacity = 0;
            await Task.Delay(150);
            SetImageToBackground();
            
            if (IsMouseOver)
                SetPolylineVisibility(_isPolyline);
        }
        else
        {
            FreezeBgImage();
        }
    }

    private void CloseAllFullscreenGrabs()
    {
        foreach (var window in Application.Current.Windows)
            if (window is ScreenGrabView sgv)
                sgv.Close();
        OnGrabClose?.Invoke();
    }

    private void FreezeUnfreezeAllScreenGrabs()
    {
        foreach (var window in Application.Current.Windows)
            if (window is ScreenGrabView sgv)
                sgv.FreezeUnfreeze();
    }
    
    private void FreezeBgImage()
    {
        BackgroundImage.Source?.Freeze();
        BackgroundImage.Source = null;
    }
    
    private void SetPolylineVisibility(bool isVisible)
    {
        HorizontalLine.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        VerticalLine.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    #endregion

    #region Mouse Events
    
    private void RegionClickCanvas_MouseLeave(object sender, MouseEventArgs e)
    {
        SetPolylineVisibility(false);
    }

    private void RegionClickCanvas_MouseEnter(object sender, MouseEventArgs e)
    {
        SetPolylineVisibility(_isPolyline);
    }

    private void RegionClickCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // Right click to close
        if (e.RightButton == MouseButtonState.Pressed)
        {
            CloseAllFullscreenGrabs();
            return;
        }

        _isSelecting = true;
        RegionClickCanvas.CaptureMouse();
        CursorClipper.ClipCursor(this);
        _clickedPoint = e.GetPosition(this);
        _selectBorder.Height = 1;
        _selectBorder.Width = 1;

        _dpiScale = VisualTreeHelper.GetDpi(this);

        try
        {
            RegionClickCanvas.Children.Remove(_selectBorder);
        }
        catch (Exception)
        {
            // ignored
        }

        _selectBorder.BorderThickness = new Thickness(2);
        var borderColor = Color.FromArgb(255, 175, 103, 193);
        _selectBorder.BorderBrush = new SolidColorBrush(borderColor);
        _ = RegionClickCanvas.Children.Add(_selectBorder);
        Canvas.SetLeft(_selectBorder, _clickedPoint.X);
        Canvas.SetTop(_selectBorder, _clickedPoint.Y);

        DisplayInfo[] screens = DisplayInfo.AllDisplayInfos;
        Point formsPoint = new((int)_clickedPoint.X, (int)_clickedPoint.Y);
        foreach (var scr in screens)
        {
            var bound = scr.ScaledBounds();
            if (bound.Contains(formsPoint))
                CurrentScreen = scr;
        }
    }

    private void RegionClickCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        var movingPoint = e.GetPosition(this);

        if (!_isSelecting)
        {
            // Update the horizontal line to match the mouse Y position
            HorizontalLine.Y1 = movingPoint.Y;
            HorizontalLine.Y2 = movingPoint.Y;

            // Update the vertical line to match the mouse X position
            VerticalLine.X1 = movingPoint.X;
            VerticalLine.X2 = movingPoint.X;
            return;
        }
        
        // Hide the lines
        HorizontalLine.Visibility = Visibility.Collapsed;
        VerticalLine.Visibility = Visibility.Collapsed;

        if (Keyboard.Modifiers == ModifierKeys.Shift)
        {
            PanSelection(movingPoint);
            return;
        }

        _isShiftDown = false;

        var left = Math.Min(_clickedPoint.X, movingPoint.X);
        var top = Math.Min(_clickedPoint.Y, movingPoint.Y);

        _selectBorder.Height = Math.Max(_clickedPoint.Y, movingPoint.Y) - top;
        _selectBorder.Width = Math.Max(_clickedPoint.X, movingPoint.X) - left;
        _selectBorder.Height += 2;
        _selectBorder.Width += 2;

        ClippingGeometry.Rect = new Rect(
            new Point(left, top),
            new Size(_selectBorder.Width - 2, _selectBorder.Height - 2));
        Canvas.SetLeft(_selectBorder, left - 1);
        Canvas.SetTop(_selectBorder, top - 1);
    }

    private void RegionClickCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isSelecting)
            return;

        _isSelecting = false;
        CurrentScreen = null;
        CursorClipper.UnClipCursor();
        RegionClickCanvas.ReleaseMouseCapture();
        ClippingGeometry.Rect = new Rect(new Point(0, 0), new Size(0, 0));

        var movingPoint = e.GetPosition(this);
        var m = PresentationSource.FromVisual(this)!.CompositionTarget!.TransformToDevice;
        movingPoint.X *= m.M11;
        movingPoint.Y *= m.M22;

        movingPoint.X = Math.Round(movingPoint.X);
        movingPoint.Y = Math.Round(movingPoint.Y);

        var xDimScaled = Canvas.GetLeft(_selectBorder) * m.M11;
        var yDimScaled = Canvas.GetTop(_selectBorder) * m.M22;

        Rectangle regionScaled = new(
            (int)xDimScaled,
            (int)yDimScaled,
            (int)(_selectBorder.Width * m.M11),
            (int)(_selectBorder.Height * m.M22));

        try
        {
            RegionClickCanvas.Children.Remove(_selectBorder);
        }
        catch
        {
            // ignored
        }

        var absPosPoint = this.GetAbsolutePosition();

        var thisCorrectedLeft = (int)absPosPoint.X + regionScaled.Left;
        var thisCorrectedTop = (int)absPosPoint.Y + regionScaled.Top;

        Rectangle correctedRegion = new(thisCorrectedLeft, thisCorrectedTop, regionScaled.Width, regionScaled.Height);
        var bitmap = correctedRegion.GetRegionOfScreenAsBitmap();
        _onImageCaptured?.Invoke(bitmap);

        CloseAllFullscreenGrabs();
    }

    private void PanSelection(Point movingPoint)
    {
        if (!_isShiftDown)
        {
            _shiftPoint = movingPoint;
            _selectLeft = Canvas.GetLeft(_selectBorder);
            _selectTop = Canvas.GetTop(_selectBorder);
        }

        _isShiftDown = true;
        _xShiftDelta = movingPoint.X - _shiftPoint.X;
        _yShiftDelta = movingPoint.Y - _shiftPoint.Y;

        var leftValue = _selectLeft + _xShiftDelta;
        var topValue = _selectTop + _yShiftDelta;

        if (CurrentScreen is not null && _dpiScale is not null)
        {
            double currentScreenLeft = CurrentScreen.Bounds.Left; // Should always be 0
            var currentScreenRight = CurrentScreen.Bounds.Right / _dpiScale.Value.DpiScaleX;
            double currentScreenTop = CurrentScreen.Bounds.Top; // Should always be 0
            var currentScreenBottom = CurrentScreen.Bounds.Bottom / _dpiScale.Value.DpiScaleY;

            leftValue = Math.Clamp(leftValue, currentScreenLeft, currentScreenRight - _selectBorder.Width);
            topValue = Math.Clamp(topValue, currentScreenTop, currentScreenBottom - _selectBorder.Height);
        }

        ClippingGeometry.Rect = new Rect(
            new Point(leftValue, topValue),
            new Size(_selectBorder.Width - 2, _selectBorder.Height - 2));
        Canvas.SetLeft(_selectBorder, leftValue - 1);
        Canvas.SetTop(_selectBorder, topValue - 1);
    }

    #endregion
}