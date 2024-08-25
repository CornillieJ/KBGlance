using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using keyboardglance.HelperWindows;
using Newtonsoft.Json;
using CheckBox = System.Windows.Controls.CheckBox;
using MessageBox = System.Windows.MessageBox;

namespace keyboardglance;

public partial class ConfigWindow : Window
{
    private const string ComboPath = @"./config/KeyCombo.json";
    private const string SizePath = @"./config/Size.json";
    private const string LocationPath = @"./config/Location.json";
    private string _keyCombo = "";
    private Dictionary<string, bool> _checkBoxState = new();
    private (double, double) _size;
    private (double, double) _location;
    public ConfigWindow()
    {
        InitializeComponent();
        FillDictionary();
        LoadConfig();
    }

    #region Event handlers
    private void BtnStart_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (var name in _checkBoxState.Keys)
        {
            CheckBox checkBox = (CheckBox)FindName(name) ?? new CheckBox();
            _checkBoxState[name] = checkBox.IsChecked??false;
        }
        var checkedBoxes = _checkBoxState.Where(pair => pair.Value).Select(pair=>pair.Key).ToList();
        _keyCombo = string.Join(",", checkedBoxes.Select(name => name.Substring(3).ToUpper()));
        WriteToJson(_checkBoxState, ComboPath);
        WriteToJson(_size, SizePath);
        WriteToJson(_location, LocationPath);
        MainWindow mainWindow = new MainWindow(_keyCombo,_size,_location);
        mainWindow.Show();
        Close();
    }
    private void BtnClose_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
    private void BtnPlacement_OnClick(object sender, RoutedEventArgs e)
    {
        PlacementWindow placement = new(200, 200);
        placement.ShowDialog();
        _size.Item1 = placement.Width;
        _size.Item2 = placement.Height;
        _location.Item1 = placement.Left;
        _location.Item2 = placement.Top;
    }
    private void BtnImages_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe" , @"resources");
    }
    private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
    #endregion
    
    
    #region Load methods
    private void FillDictionary()
    {
        var allCheckBoxes = stkLeftKeys.Children.Cast<CheckBox>()
            .Concat(stkRightKeys.Children.Cast<CheckBox>());
        foreach (var checkBox in allCheckBoxes)
        {
            _checkBoxState.TryAdd(checkBox.Name,false);
        }
    } 
    private void LoadConfig()
    {
        try
        {
            CheckInitialCheckbox();
            LoadPlacement();
        }
        catch (Exception)
        {
            MessageBox.Show("Config files are corrupt. delete config folder and restart");
        }
    } 
    private void CheckInitialCheckbox()
    {
        chkRalt.IsChecked = true;
        chkRctrl.IsChecked = true;
        if (!File.Exists(ComboPath)) return;
        _checkBoxState = ReadFromJson<Dictionary<string, bool>>(ComboPath);
        foreach (var boxState in _checkBoxState)
        {
            ((CheckBox)FindName(boxState.Key)).IsChecked = boxState.Value;
        }
    }
    private void LoadPlacement()
    {
        if (File.Exists(SizePath))
            _size = ReadFromJson<(double, double)>(SizePath);
        if (File.Exists(LocationPath))
            _location = ReadFromJson<(double, double)>(LocationPath);
    }
    #endregion
    #region json methods
    private void WriteToJson(object writeObject,string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        var json = JsonConvert.SerializeObject(writeObject, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
    private T ReadFromJson<T>(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<T>(json);
    } 
    #endregion
}