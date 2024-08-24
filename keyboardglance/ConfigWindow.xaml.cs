using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace keyboardglance;

public partial class ConfigWindow : Window
{
    public ConfigWindow()
    {
        InitializeComponent();
    }

    private void BtnStart_OnClick(object sender, RoutedEventArgs e)
    {
        MainWindow mainWindow = new MainWindow();
        mainWindow.Show();

        Close();
    }

    private void BtnClose_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
}