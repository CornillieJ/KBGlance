using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gma.System.MouseKeyHook;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace keyboardglance;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>  f   
public partial class MainWindow : Window
{
    private const double StartingOpacity = 0.8;
    private List<Key> _keyCombo = [Key.RightAlt,Key.RightCtrl];
    private Dictionary<Key, Uri> _layerKeyMap = new ()
    {
        {Key.D0,new Uri("resources/layer_0.png",UriKind.Relative)},
        {Key.D1,new Uri("resources/layer_1.png",UriKind.Relative)},
        {Key.D2,new Uri("resources/layer_2.png",UriKind.Relative)},
        {Key.D3,new Uri("resources/layer_3.png",UriKind.Relative)},
        {Key.D4,new Uri("resources/layer_4.png",UriKind.Relative)},
        {Key.D5,new Uri("resources/layer_5.png",UriKind.Relative)},
        {Key.D6,new Uri("resources/layer_6.png",UriKind.Relative)},
        {Key.D7,new Uri("resources/layer_7.png",UriKind.Relative)},
    };
    private Dictionary<Key,bool> _pressedKeys = new(); 
    private IKeyboardMouseEvents _globalHook;
    private int _windowDelay=100;
    private int _currentDelay;
    private CancellationTokenSource _cancellationTokenSource = new();

    public MainWindow()
    {
        InitializeComponent();
        HookKeyboard();
        HideWindow(_cancellationTokenSource.Token);
        Left = 10;
        Top = 10;
    }
    
    private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
    {
        _pressedKeys.TryAdd(e.Key,true);
        if(WindowState == WindowState.Normal) return;
        if (!IsComboPressed()) return;
        Uri layerUri = GetLayerUri()?? throw new InvalidOperationException();
        ShowWindow(layerUri);
    }

    private Uri? GetLayerUri()
    {
        foreach (Key key in _pressedKeys.Keys)
        {
            if (_layerKeyMap.TryGetValue(key, out Uri? result)) return result;
        }

        return null;
    }

    private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
    {
        if (!_pressedKeys.ContainsKey(e.Key)) return;
        _pressedKeys.Remove(e.Key);
        if(IsComboPressed()) return;
        HideWindow(_cancellationTokenSource.Token);
    }
    protected override void OnClosed(EventArgs e)
    {
        _globalHook.KeyDown -= GlobalHook_KeyDown;
        _globalHook.KeyUp -= GlobalHook_KeyUp;
        _globalHook.Dispose();
        base.OnClosed(e);
    }
    private void HookKeyboard()
    {
        _globalHook = Hook.GlobalEvents();
        _globalHook.KeyDown += GlobalHook_KeyDown;
        _globalHook.KeyUp += GlobalHook_KeyUp;
    }
    private void GlobalHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        Key key = KeyInterop.KeyFromVirtualKey(e.KeyValue);
        KeyEventArgs newE = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(this), 0, key);
        MainWindow_OnKeyDown(sender,newE);
    }

    private void GlobalHook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        Key key = KeyInterop.KeyFromVirtualKey(e.KeyValue);
        KeyEventArgs newE = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(this), 0, key);
        MainWindow_OnKeyUp(sender,newE);
    }

    private void ShowWindow(Uri layerUri)
    {
        _cancellationTokenSource.Cancel();
        imgLayer.Source = new BitmapImage(layerUri);
        Opacity = StartingOpacity;
        _currentDelay = 0;
        Show();
        WindowState = WindowState.Normal;
        Topmost = true;
        Activate();
        Focusable = false;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private bool IsComboPressed()
    {
        bool isModifierPressed = _keyCombo.All(k => _pressedKeys.ContainsKey(k));
        bool isNumberPressed = _layerKeyMap.Any(pair => _pressedKeys.ContainsKey(pair.Key));
        return isModifierPressed && isNumberPressed;
    }

    private async void HideWindow(CancellationToken token)
    {
        while (_currentDelay < _windowDelay && !token.IsCancellationRequested)
        {
            _currentDelay++;
            await Task.Delay(10);
            Opacity = (_windowDelay - _currentDelay) / (double)_windowDelay;
        }
        _currentDelay = 0;
        if (token.IsCancellationRequested) return;
        WindowState = WindowState.Minimized;
        Hide();
        Opacity = StartingOpacity;
    }

}