using System.Windows;
using System.Windows.Input;

namespace keyboardglance.HelperWindows;

public partial class PlacementWindow : Window
{
    private readonly double _aspectRatio;
    private double _originalButtonWidth;

    public PlacementWindow(int width, int height, double? aspectRatio = null)
    {
        InitializeComponent();
        this.Width = width;
        this.Height = height;
        _aspectRatio = aspectRatio??0;
        _originalButtonWidth = btnConfirm.Width;
    }

    private void PlacementWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void BtnConfirm_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void PlacementWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (Width <= btnConfirm.Width || Height <= btnConfirm.Height)
        {
            btnConfirm.Width = Width - 10;
            btnConfirm.Height = Height - 10;
            btnConfirm.FontSize = 24 * (_originalButtonWidth / Width - 10);
        }
    }
}