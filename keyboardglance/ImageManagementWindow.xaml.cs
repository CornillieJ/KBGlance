using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace keyboardglance;

public partial class ImageManagementWindow : Window
{
    public Dictionary<int, string> _layers = new();
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "Layers.config");
    private static readonly string ResourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources");
    private const int ImagesPerRow = 3;
    
    [GeneratedRegex(@"(\d+)")]
    private static partial Regex LayerNumberRegex();
    public ImageManagementWindow()
    {
        InitializeComponent();
        _layers = InitializeLayers();
        ShowImages();
    }
    private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if(e.ChangedButton == MouseButton.Left)
            DragMove();
    }
    private void BtnImageFolder_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe", @"resources");
    }

    private void ImageButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Controls.ImageButton imageButton)
        {
            // Create OpenFileDialog to pick a new image
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select Image",
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources")
            };

            if (dialog.ShowDialog() == true)
            {
                string selectedFilePath = dialog.FileName;
                string fileName = $"layer_{imageButton.LayerNumber}{Path.GetExtension(selectedFilePath)}";
                string destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", fileName);
                string tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", Guid.NewGuid() + Path.GetExtension(selectedFilePath));
                string lastFile = imageButton.ImageSource;
                
                _layers[imageButton.LayerNumber] = destPath;

                try
                {
                    if (selectedFilePath == destPath) return;
                    imageButton.ClearImage();
                    stkImages.Children.Clear();
                    File.Copy(selectedFilePath, tempPath, true);
                    // Force immediate garbage collection
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // Try to delete the old file with error handling
                    if (File.Exists(lastFile))
                        File.Delete(lastFile);
                    
                    File.Copy(tempPath, destPath, true);
                    File.Delete(tempPath);
                    
                    _layers[imageButton.LayerNumber] = destPath;
                    SaveLayers(_layers);
                    ShowImages();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
    private static void SaveLayers(Dictionary<int,string> layersDict)
    {
        try
        {
            var directory = Path.GetDirectoryName(ConfigFilePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(layersDict, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(ConfigFilePath, json);
            MainWindow.Layers = layersDict;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving layers configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public static Dictionary<int,string> InitializeLayers()
    {
        Dictionary<int,string> layerDict = new();
        if (File.Exists(ConfigFilePath))
        {
            try
            {
                string json = File.ReadAllText(ConfigFilePath);
                layerDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, string>>(json) ?? new Dictionary<int, string>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading layers configuration: {ex.Message}\nCreating new configuration.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // If no config, scan resources folder for existing images
        var imageFiles = FindImages();
        foreach (var imageFile in imageFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(imageFile);
            if (LayerNumberRegex().IsMatch(fileName))
            {
                int layerNumber = int.Parse(LayerNumberRegex().Match(fileName).Groups[1].Value);
                layerDict[layerNumber] = imageFile;
            }
        }
        SaveLayers(layerDict);
        return layerDict;
    }
    
    private static List<string> FindImages()
    {
        //get all files in subfolder "resources"
        if (!Directory.Exists(ResourcesPath))
            Directory.CreateDirectory(ResourcesPath);

        //filter for images
        string[] supportedTypes = { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
        return Directory.GetFiles(ResourcesPath)
            .Where(file => supportedTypes.Contains(Path.GetExtension(file).ToLower()))
            .ToList();
    }
    
    private void ShowImages()
    {
        stkImages.Children.Clear();

        // Create StackPanel with horizontal orientation to hold images in rows
        StackPanel currentRow = new StackPanel { Orientation = Orientation.Horizontal };
        stkImages.Children.Add(currentRow);

        foreach (var layerImage in _layers)
        {
            if (layerImage.Key % ImagesPerRow == 0)
            {
                currentRow = new StackPanel { Orientation = Orientation.Horizontal };
                stkImages.Children.Add(currentRow);
            }

            // Create image button using the custom control
            Controls.ImageButton imageButton = new Controls.ImageButton()
            {
                ImageSource = layerImage.Value,
            };
            imageButton.SetSize(200, 200);
            imageButton.ButtonClick += ImageButton_Click;
            imageButton.SetLayerNumber(layerImage.Key);

            // Add to current row
            currentRow.Children.Add(imageButton);
        }
    }
    private void BtnClose_OnClick(object sender, RoutedEventArgs e)
    {
        SaveLayers(_layers);
        Close();
    }
    private void BtnFindImages_OnClick(object sender, RoutedEventArgs e)
    {
        var imageFiles = FindImages();
        foreach (var imageFile in imageFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(imageFile);
            if (LayerNumberRegex().IsMatch(fileName))
            {
                int layerNumber = int.Parse(LayerNumberRegex().Match(fileName).Groups[1].Value);
                _layers[layerNumber] = imageFile;
            }
        }
        SaveLayers(_layers);
    }
}