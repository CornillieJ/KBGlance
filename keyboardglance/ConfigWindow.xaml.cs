using System.IO;
using System.Net;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Newtonsoft.Json;
using CheckBox = System.Windows.Controls.CheckBox;
namespace keyboardglance;

public partial class ConfigWindow : Window
{
    private const string ComboPath = @"./config/KeyCombo.json";
    private string _keyCombo = "";
    private Dictionary<string, bool> _checkBoxState = new();
    public ConfigWindow()
    {
        InitializeComponent();
        FillDictionary();
        CheckInitialCheckbox();
    }
    private void BtnStart_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (var name in _checkBoxState.Keys)
        {
            CheckBox checkBox = (CheckBox)FindName(name) ?? new CheckBox();
            _checkBoxState[name] = checkBox.IsChecked??false;
        }
        var checkedBoxes = _checkBoxState.Where(pair => pair.Value).Select(pair=>pair.Key).ToList();
        WriteToJson(_checkBoxState,ComboPath);   
        _keyCombo = string.Join(",", checkedBoxes.Select(name => name.Substring(3).ToUpper()));
        MainWindow mainWindow = new MainWindow(_keyCombo);
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
    private void FillDictionary()
    {
        var allCheckBoxes = stkLeftKeys.Children.Cast<CheckBox>()
            .Concat(stkRightKeys.Children.Cast<CheckBox>());
        foreach (var checkBox in allCheckBoxes)
        {
            _checkBoxState.TryAdd(checkBox.Name,false);
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
}