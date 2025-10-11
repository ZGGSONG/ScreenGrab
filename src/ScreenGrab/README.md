# Application Method

## Usage

1. Callback method after screenshot is completed

```csharp
BitmapSource bs;
bool isAuxiliary = true; // Open auxiliary lines
ScreenGrabber.OnCaptured = bitmap => bs = bitmap.ToImageSource();
ScreenGrabber.Capture(isAuxiliary);
```

2. Synchronous method to get screenshot(like MessageBox)

```csharp
bool isAuxiliary = false; // Do not open auxiliary lines
var bitmap = ScreenGrabber.CaptureDialog(isAuxiliary);
```

3. Asynchronous method to get screenshot

```csharp
bool isAuxiliary = false; // Do not open auxiliary lines
var bitmap = await ScreenGrabber.CaptureAsync(isAuxiliary);
```