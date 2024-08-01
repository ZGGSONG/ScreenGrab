# Application Method

## Usage

```csharp
BitmapSource bs;
bool isAuxiliary = true; // Open auxiliary lines
ScreenGrabber.OnCaptured = bitmap => bs = bitmap.ToImageSource();
ScreenGrabber.Capture(isAuxiliary);
```