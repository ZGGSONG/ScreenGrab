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
///     Extracted from the project <see href="https://github.com/TheJoeFin/Text-Grab" />
/// </summary>
public partial class ScreenGrabView
{
    #region Constructors

    public ScreenGrabView(Action<Bitmap>? action, bool isAuxiliary = false)
    {
        InitializeComponent();
        _onImageCaptured = action;
        _isAuxiliary = isAuxiliary;
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
    private double _selectLeft;
    private double _selectTop;
    private Point _shiftPoint;
    private double _xShiftDelta;
    private double _yShiftDelta;
    private Point _promptMsgTopLeft;
    private Point _promptMsgBottomRight;

    private readonly Border _selectBorder = new();
    private const double SelectBorderThickness = 2;
    private readonly Color _borderColor = Color.FromArgb(255, 146, 202, 244);

    private readonly Action<Bitmap>? _onImageCaptured;
    private readonly bool _isAuxiliary;

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

        if (this.IsMouseInWindow())
            SetPromptMsgVisibility(true);

        if (!_isAuxiliary) return;
        (HorizontalLine.X1, VerticalLine.Y1, (HorizontalLine.X2, VerticalLine.Y2)) = (0, 0, this.GetWidthHeight());
        
        // Set the Auxiliary to be visible when the mouse in the window
        if (this.IsMouseInWindow())
            SetAuxiliaryVisibility(true);
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
                CloseAllScreenGrabs();
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
            if (_isAuxiliary) SetAuxiliaryVisibility(false);
            SetPromptMsgVisibility(false);
            BackgroundBrush.Opacity = 0;
            await Task.Delay(150);
            SetImageToBackground();

            if (this.IsMouseInWindow())
            {
                SetAuxiliaryVisibility(_isAuxiliary);
                SetPromptMsgVisibility(true);
            }
        }
        else
        {
            FreezeBgImage();
        }
    }

    private void CloseAllScreenGrabs()
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

    private void SetAuxiliaryVisibility(bool isVisible)
    {
        var setVisibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        if (HorizontalLine.Visibility == setVisibility && VerticalLine.Visibility == setVisibility)
            return;
        HorizontalLine.Visibility = setVisibility;
        VerticalLine.Visibility = setVisibility;
    }

    private void SetPromptMsgVisibility(bool isVisible)
    {
        var setVisibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        if (PromptMsg.Visibility == setVisibility)
            return;

        PromptMsg.Visibility = setVisibility;
    }

    #endregion

    #region Mouse Events

    private void RegionClickCanvas_MouseLeave(object sender, MouseEventArgs e)
    {
        SetAuxiliaryVisibility(false);
        SetPromptMsgVisibility(false);
    }

    private void RegionClickCanvas_MouseEnter(object sender, MouseEventArgs e)
    {
        SetAuxiliaryVisibility(_isAuxiliary);
        SetPromptMsgVisibility(true);

        // 获取 PromptMsg 控件的边界
        _promptMsgTopLeft = PromptMsg.TranslatePoint(new Point(0, 0), this);
        _promptMsgBottomRight = PromptMsg.TranslatePoint(new Point(PromptMsg.ActualWidth, PromptMsg.ActualHeight), this);
    }

    private void RegionClickCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // Right click to close
        if (e.RightButton == MouseButtonState.Pressed)
        {
            CloseAllScreenGrabs();
            return;
        }

        _isSelecting = true;
        SetAuxiliaryVisibility(false);
        SetPromptMsgVisibility(false);
        RegionClickCanvas.CaptureMouse();
        CursorClipper.ClipCursor(this);
        _clickedPoint = e.GetPosition(this);
        _selectBorder.Height = 2;
        _selectBorder.Width = 2;

        _dpiScale = VisualTreeHelper.GetDpi(this);

        try
        {
            RegionClickCanvas.Children.Remove(_selectBorder);
        }
        catch (Exception)
        {
            // ignored
        }

        _selectBorder.BorderThickness = new Thickness(SelectBorderThickness);
        _selectBorder.BorderBrush = new SolidColorBrush(_borderColor);
        _ = RegionClickCanvas.Children.Add(_selectBorder);
        Canvas.SetLeft(_selectBorder, _clickedPoint.X);
        Canvas.SetTop(_selectBorder, _clickedPoint.Y);
        
        // Initialize ClippingGeometry.Rect with a valid Rect
        ClippingGeometry.Rect = new Rect(_clickedPoint, new Size(0, 0));        
        
        WindowUtilities.GetMousePosition(out var mousePoint);
        foreach (var screen in DisplayInfo.AllDisplayInfos)
        {
            var bound = screen.ScaledBounds();
            if (bound.Contains(mousePoint))
                CurrentScreen = screen;
        }
    }

    private void RegionClickCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        var movingPoint = e.GetPosition(this);

        if (!_isSelecting)
        {
            // 检查鼠标是否在 PromptMsg 控件区域内
            var isMouseInPromptMsg = movingPoint.X >= _promptMsgTopLeft.X && movingPoint.X <= _promptMsgBottomRight.X &&
                                     movingPoint.Y >= _promptMsgTopLeft.Y && movingPoint.Y <= _promptMsgBottomRight.Y;

            SetPromptMsgVisibility(!isMouseInPromptMsg);

            // Determine whether to update auxiliary line information based on configuration
            if (!_isAuxiliary) return;

            // Update the horizontal line to match the mouse Y position
            HorizontalLine.Y1 = HorizontalLine.Y2 = movingPoint.Y;

            // Update the vertical line to match the mouse X position
            VerticalLine.X1 = VerticalLine.X2 = movingPoint.X;
            return;
        }

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
        _selectBorder.Height += SelectBorderThickness;
        _selectBorder.Width += SelectBorderThickness;

        ClippingGeometry.Rect = new Rect(
            new Point(left, top),
            new Size(_selectBorder.Width - SelectBorderThickness, _selectBorder.Height - SelectBorderThickness));
        Canvas.SetLeft(_selectBorder, left - 1);
        Canvas.SetTop(_selectBorder, top - 1);
    }

    private void RegionClickCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isSelecting || _dpiScale is null)
            return;

        _isSelecting = false;
        CurrentScreen = null;
        CursorClipper.UnClipCursor();
        RegionClickCanvas.ReleaseMouseCapture();
        ClippingGeometry.Rect = new Rect(new Point(0, 0), new Size(0, 0));

        // Get the scaled dimensions of the selection border except the border thickness
        var xDimScaled = (Canvas.GetLeft(_selectBorder) + SelectBorderThickness) * _dpiScale.Value.DpiScaleX;
        var yDimScaled = (Canvas.GetTop(_selectBorder) + SelectBorderThickness) * _dpiScale.Value.DpiScaleY;

        // Get the scaled dimensions of the selection border except the border thickness
        Rectangle regionScaled = new(
            (int)xDimScaled,
            (int)yDimScaled,
            (int)((_selectBorder.Width - 2 * SelectBorderThickness) * _dpiScale.Value.DpiScaleX),
            (int)((_selectBorder.Height - 2 * SelectBorderThickness) * _dpiScale.Value.DpiScaleY));

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

        var correctedRegion = regionScaled with { X = thisCorrectedLeft, Y = thisCorrectedTop };
        var bitmap = correctedRegion.GetRegionOfScreenAsBitmap();
        CloseAllScreenGrabs();

        _onImageCaptured?.Invoke(bitmap);
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
            double currentScreenLeft = 0;
            double currentScreenTop = 0;
            var currentScreenRight = CurrentScreen.Bounds.Width / _dpiScale.Value.DpiScaleX;
            var currentScreenBottom = CurrentScreen.Bounds.Height / _dpiScale.Value.DpiScaleY;

            leftValue = Clamp(leftValue, currentScreenLeft, currentScreenRight - _selectBorder.Width);
            topValue = Clamp(topValue, currentScreenTop, currentScreenBottom - _selectBorder.Height);
        }

        ClippingGeometry.Rect = new Rect(
            new Point(leftValue, topValue),
            new Size(_selectBorder.Width - 2, _selectBorder.Height - 2));
        Canvas.SetLeft(_selectBorder, leftValue - 1);
        Canvas.SetTop(_selectBorder, topValue - 1);
    }

    private double Clamp(double value, double min, double max)
    {
#if NETFRAMEWORK
        if (value < min) return min;
        if (value > max) return max;
        return value;
#else
        return Math.Clamp(value, min, max);
#endif
    }

    #endregion
}