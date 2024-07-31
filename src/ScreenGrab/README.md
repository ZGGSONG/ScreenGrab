# Application Method

## Usage

```csharp
BitmapSource bs;
bool isPolyline = true; // Open auxiliary lines
ScreenGrabber.OnCaptured = bitmap => bs = bitmap.ToImageSource();
ScreenGrabber.Capture(isPolyline);
```