using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dapplo.Windows.User32;
using ScreenGrab.Extensions;
using ScreenGrab.Utilities;
using Point = System.Windows.Point;

namespace ScreenGrab;

public partial class Grab
{
    #region Fields

    private Point clickedPoint = new();
    private TextBox? destinationTextBox;
    private DpiScale? dpiScale;
    private bool isSelecting = false;
    private bool isShiftDown = false;
    private Border selectBorder = new();
    private double selectLeft;
    private double selectTop;
    private Point shiftPoint = new();
    private double xShiftDelta;
    private double yShiftDelta;
    private bool _isFreeze = true;

    #endregion Fields

    #region Constructors

    public Grab()
    {
        InitializeComponent();
    }

    #endregion Constructors

    #region Properties
    private DisplayInfo? CurrentScreen { get; set; }
    
    public Bitmap Image { get; set; }

    #endregion Properties

    #region Window Events

    public void SetImageToBackground()
    {
        BackgroundImage.Source = null;
        BackgroundImage.Source = ImageMethods.GetWindowBoundsImage(this);
        BackgroundBrush.Opacity = 0.2;
    }
    
    private async void FreezeUnfreeze(bool isActivate)
    {
        if (isActivate)
        {
            BackgroundBrush.Opacity = 0;
            await Task.Delay(150);
            SetImageToBackground();
        }
        else
        {
            BackgroundImage.Source = null;
        }
    }

    private void FullscreenGrab_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                DialogResult = false;
                Close();
                break;
            case Key.F:
                //TODO: 添加全局变量控制显隐
                _isFreeze = !_isFreeze;
                FreezeUnfreeze(_isFreeze);
                break;
            default:
                break;
        }
    }

    private void FullscreenGrab_KeyUp(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.LeftShift:
            case Key.RightShift:
                isShiftDown = false;
                clickedPoint = new Point(clickedPoint.X + xShiftDelta, clickedPoint.Y + yShiftDelta);
                break;
            default:
                break;
        }
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        Close();
        
        GC.Collect();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Maximized;
        FullWindow.Rect = new System.Windows.Rect(0, 0, Width, Height);
        KeyDown += FullscreenGrab_KeyDown;
        KeyUp += FullscreenGrab_KeyUp;

        SetImageToBackground();

#if DEBUG
        Topmost = false;
#endif
    }

    private void Window_Unloaded(object sender, RoutedEventArgs e)
    {
        BackgroundImage.Source = null;
        BackgroundImage.UpdateLayout();
        CurrentScreen = null;
        dpiScale = null;

        Loaded -= Window_Loaded;
        Unloaded -= Window_Unloaded;

        RegionClickCanvas.MouseDown -= RegionClickCanvas_MouseDown;
        RegionClickCanvas.MouseMove -= RegionClickCanvas_MouseMove;
        RegionClickCanvas.MouseUp -= RegionClickCanvas_MouseUp;

        KeyDown -= FullscreenGrab_KeyDown;
        KeyUp -= FullscreenGrab_KeyUp;
    }

    #endregion

    #region Mouse Events

    private void RegionClickCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.RightButton == MouseButtonState.Pressed)
            return;

        isSelecting = true;
        RegionClickCanvas.CaptureMouse();
        CursorClipper.ClipCursor(this);
        clickedPoint = e.GetPosition(this);
        selectBorder.Height = 1;
        selectBorder.Width = 1;

        dpiScale = VisualTreeHelper.GetDpi(this);

        try
        {
            RegionClickCanvas.Children.Remove(selectBorder);
        }
        catch (Exception)
        {
            // ignored
        }

        selectBorder.BorderThickness = new Thickness(2);
        System.Windows.Media.Color borderColor = System.Windows.Media.Color.FromArgb(255, 40, 118, 126);
        selectBorder.BorderBrush = new SolidColorBrush(borderColor);
        _ = RegionClickCanvas.Children.Add(selectBorder);
        Canvas.SetLeft(selectBorder, clickedPoint.X);
        Canvas.SetTop(selectBorder, clickedPoint.Y);

        DisplayInfo[] screens = DisplayInfo.AllDisplayInfos;
        System.Windows.Point formsPoint = new((int)clickedPoint.X, (int)clickedPoint.Y);
        foreach (DisplayInfo scr in screens)
        {
            Rect bound = scr.ScaledBounds();
            if (bound.Contains(formsPoint))
                CurrentScreen = scr;
        }
    }

    private void RegionClickCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!isSelecting)
            return;

        Point movingPoint = e.GetPosition(this);

        if (Keyboard.Modifiers == ModifierKeys.Shift)
        {
            PanSelection(movingPoint);
            return;
        }

        isShiftDown = false;

        double left = Math.Min(clickedPoint.X, movingPoint.X);
        double top = Math.Min(clickedPoint.Y, movingPoint.Y);

        selectBorder.Height = Math.Max(clickedPoint.Y, movingPoint.Y) - top;
        selectBorder.Width = Math.Max(clickedPoint.X, movingPoint.X) - left;
        selectBorder.Height += 2;
        selectBorder.Width += 2;

        clippingGeometry.Rect = new Rect(
            new System.Windows.Point(left, top),
            new System.Windows.Size(selectBorder.Width - 2, selectBorder.Height - 2));
        Canvas.SetLeft(selectBorder, left - 1);
        Canvas.SetTop(selectBorder, top - 1);
    }

    private void RegionClickCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!isSelecting)
            return;

        isSelecting = false;
        CurrentScreen = null;
        CursorClipper.UnClipCursor();
        RegionClickCanvas.ReleaseMouseCapture();
        clippingGeometry.Rect = new Rect(
            new System.Windows.Point(0, 0),
            new System.Windows.Size(0, 0));

        System.Windows.Point movingPoint = e.GetPosition(this);
        Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
        movingPoint.X *= m.M11;
        movingPoint.Y *= m.M22;

        movingPoint.X = Math.Round(movingPoint.X);
        movingPoint.Y = Math.Round(movingPoint.Y);

        double xDimScaled = Canvas.GetLeft(selectBorder) * m.M11;
        double yDimScaled = Canvas.GetTop(selectBorder) * m.M22;

        Rectangle regionScaled = new(
            (int)xDimScaled,
            (int)yDimScaled,
            (int)(selectBorder.Width * m.M11),
            (int)(selectBorder.Height * m.M22));
        
        try
        {
            RegionClickCanvas.Children.Remove(selectBorder);
        }
        catch
        {
            // ignored
        }

        //FIXME: ZGGSONG
        Point absPosPoint = this.GetAbsolutePosition();

        int thisCorrectedLeft = (int)absPosPoint.X + regionScaled.Left;
        int thisCorrectedTop = (int)absPosPoint.Y + regionScaled.Top;

        Rectangle correctedRegion = new(thisCorrectedLeft, thisCorrectedTop, regionScaled.Width, regionScaled.Height);
        Image = ImageMethods.GetRegionOfScreenAsBitmap(correctedRegion);

        DialogResult = true;
    }

    private void PanSelection(Point movingPoint)
    {
        if (!isShiftDown)
        {
            shiftPoint = movingPoint;
            selectLeft = Canvas.GetLeft(selectBorder);
            selectTop = Canvas.GetTop(selectBorder);
        }

        isShiftDown = true;
        xShiftDelta = (movingPoint.X - shiftPoint.X);
        yShiftDelta = (movingPoint.Y - shiftPoint.Y);

        double leftValue = selectLeft + xShiftDelta;
        double topValue = selectTop + yShiftDelta;

        if (CurrentScreen is not null && dpiScale is not null)
        {
            double currentScreenLeft = CurrentScreen.Bounds.Left; // Should always be 0
            double currentScreenRight = CurrentScreen.Bounds.Right / dpiScale.Value.DpiScaleX;
            double currentScreenTop = CurrentScreen.Bounds.Top; // Should always be 0
            double currentScreenBottom = CurrentScreen.Bounds.Bottom / dpiScale.Value.DpiScaleY;

            leftValue = Math.Clamp(leftValue, currentScreenLeft, (currentScreenRight - selectBorder.Width));
            topValue = Math.Clamp(topValue, currentScreenTop, (currentScreenBottom - selectBorder.Height));
        }

        clippingGeometry.Rect = new Rect(
            new System.Windows.Point(leftValue, topValue),
            new System.Windows.Size(selectBorder.Width - 2, selectBorder.Height - 2));
        Canvas.SetLeft(selectBorder, leftValue - 1);
        Canvas.SetTop(selectBorder, topValue - 1);
    }

    #endregion
}