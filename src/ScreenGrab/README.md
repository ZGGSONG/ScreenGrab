# Application Method

## Usage

```csharp
BitmapSource bs;
ScreenGrabber.OnCaptured = bitmap => bs = bitmap.ToImageSource();
ScreenGrabber.Capture();
```