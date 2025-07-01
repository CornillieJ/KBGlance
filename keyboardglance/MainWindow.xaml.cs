using System.Drawing;
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
using KeyEventArgs=System.Windows.Input.KeyEventArgs;

namespace keyboardglance;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>  f   
public partial class MainWindow : Window
{
    private const double StartingOpacity = 1;
    private NotifyIcon _notifyIcon;
    private const string IconLocation = "resources/notifyicon.ico";//<a href="https://www.flaticon.com/free-icons/keyboard-and-mouse" title="Keyboard and mouse icons">Keyboard and mouse icons created by Muhammad Atif - Flaticon</a> 
    private List<Key> _keyCombo = [Key.RightAlt, Key.RightCtrl];
    public static Dictionary<int, string> Layers { get; set; }

    private Dictionary<Key, int> _layerKeyMap = new()
    {
        { Key.D0, 0 }, { Key.D1, 1 }, { Key.D2, 2 }, { Key.D3, 3 }, { Key.D4, 4 },
        { Key.D5, 5 }, { Key.D6, 6 }, { Key.D7, 7 }, { Key.D8, 8 }, { Key.D9, 9 },
    };

    private Dictionary<string, Key> _comboKeyMap = new()
    {
        { "LALT", Key.LeftAlt },
        { "LSHIFT", Key.LeftShift },
        { "LCTRL", Key.LeftCtrl },
        { "LGUI", Key.LWin },
        { "RALT", Key.RightAlt },
        { "RSHIFT", Key.RightShift },
        { "RCTRL", Key.RightCtrl },
        { "RGUI", Key.RWin }
    };

    private Dictionary<Key, bool> _pressedKeys = new();
    private IKeyboardMouseEvents _globalHook;
    private int _windowDelay = 200;
    private int _currentDelay;
    private CancellationTokenSource _cancellationTokenSource = new();

    public MainWindow(string keyCombo, (double, double) size, (double, double) location)
    {
        InitializeComponent();
        InitializeNotifyIcon();
        AssignComboKeys(keyCombo);
        HookKeyboard();
        HideWindow(_cancellationTokenSource.Token);
        AdjustPlacement(size, location);
        _notifyIcon.ShowBalloonTip(1000, "Minimized to system tray", "The application is still running in the background.", ToolTipIcon.Info);

        if (Layers?.Count > 0)
            return;
        Layers = ImageManagementWindow.InitializeLayers();
    }

    #region Event handlers
    private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
    {
        _pressedKeys.TryAdd(e.Key, true);
        if (WindowState == WindowState.Normal) return;
        if (!IsComboPressed()) return;
        Uri layerUri = GetLayerUri() ?? throw new InvalidOperationException();
        ShowWindow(layerUri);
    }

    private Uri? GetLayerUri()
    {
        foreach (Key key in _pressedKeys.Keys)
        {
            if (_layerKeyMap.TryGetValue(key, out int result))
                return new Uri(Layers[result], UriKind.RelativeOrAbsolute);
        }
        return null;
    }

    private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
    {
        if (!_pressedKeys.ContainsKey(e.Key)) return;
        _pressedKeys.Remove(e.Key);
        if (IsComboPressed()) return;
        HideWindow(_cancellationTokenSource.Token);
    }
    protected override void OnClosed(EventArgs e)
    {
        _globalHook.KeyDown -= GlobalHook_KeyDown;
        _globalHook.KeyUp -= GlobalHook_KeyUp;
        _globalHook.Dispose();
        base.OnClosed(e);
    }
    private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
    {
        (new ConfigWindow()).Show();
        Close();
    }
    #endregion
    #region Keyboard hook methods
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
        MainWindow_OnKeyDown(sender, newE);
    }

    private void GlobalHook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        Key key = KeyInterop.KeyFromVirtualKey(e.KeyValue);
        KeyEventArgs newE = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(this), 0, key);
        MainWindow_OnKeyUp(sender, newE);
    }
    #endregion

    private void AssignComboKeys(string comboKeyString)
    {
        string[] combokeys = comboKeyString.Split(",");
        bool isValid = combokeys.All(k => _comboKeyMap.ContainsKey(k));
        if (!isValid) throw new ArgumentException("Invalid key");
        _keyCombo = combokeys.Select(k => _comboKeyMap[k]).ToList();
    }
    private void ShowWindow(Uri layerUri)
    {
        _cancellationTokenSource.Cancel();

        _windowDelay = 100;
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.UriSource = layerUri;
        bitmapImage.EndInit();
        imgLayer.Source = bitmapImage;


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
    private void InitializeNotifyIcon()
    {
        _notifyIcon = new NotifyIcon();
        _notifyIcon.Icon = new Icon(IconLocation);
        _notifyIcon.Text = "KB Glance";
        _notifyIcon.Visible = true;
        _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Config", null, (s, e) => {
            (new ConfigWindow()).Show();
            Close();
        });
        contextMenu.Items.Add("Exit", null, (s, e) => Close());
        _notifyIcon.ContextMenuStrip = contextMenu;
    }
    private void AdjustPlacement((double, double) size, (double, double) location)
    {
        Left = 10;
        Top = 10;
        if (size.Item1 != 0 && size.Item2 != 0)
        {
            Width = size.Item1;
            Height = size.Item2;
        }
        if (location.Item1 != 0 && location.Item2 != 0)
        {
            Left = location.Item1;
            Top = location.Item2;
        }
    }
}