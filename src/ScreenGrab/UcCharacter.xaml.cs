using System.Windows;
using System.Windows.Controls;
namespace ScreenGrab;

public partial class UcCharacter : UserControl
{
    public UcCharacter()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty DisplayCharProperty =
    DependencyProperty.Register(
        "DisplayChar",
        typeof(string),
        typeof(UcCharacter),
        new PropertyMetadata(string.Empty));

    public string DisplayChar
    {
        get => (string)GetValue(DisplayCharProperty);
        set => SetValue(DisplayCharProperty, value);
    }
}
