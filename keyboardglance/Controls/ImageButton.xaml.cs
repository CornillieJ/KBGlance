using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace keyboardglance.Controls
{
    public partial class ImageButton : UserControl
    {
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(string), typeof(ImageButton), 
                new PropertyMetadata(string.Empty, OnImageSourceChanged));
        public static readonly DependencyProperty LayerNumberProperty =
            DependencyProperty.Register(nameof(LayerNumber), typeof(int), typeof(ImageButton), 
            new PropertyMetadata(-1,OnLayerNumberChanged));

        public int LayerNumber
        {
            get => (int)GetValue(LayerNumberProperty);
            set => SetValue(LayerNumberProperty,value);
        }

        public string ImageSource
        {
            get => (string)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public void ClearImage()
        {
            var oldSource = ImageControl.Source as BitmapImage;
            ImageControl.Source = null;

            if (oldSource != null)
            {
                try {
                    oldSource.StreamSource?.Dispose();
                } catch { /* Ignore errors during cleanup */ }
            }

            ImageSource = string.Empty;
        }
        
        public event RoutedEventHandler ButtonClick;

        public ImageButton()
        {
            InitializeComponent();
        }

        private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageButton imageButton)
            {
                if (e.NewValue is string path && !string.IsNullOrEmpty(path))
                {
                    // Create BitmapImage that doesn't lock the file
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // This is key - loads image in memory and releases file lock
                    bitmapImage.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                    bitmapImage.EndInit();

                    imageButton.ImageControl.Source = bitmapImage;
                }
                else
                {
                    imageButton.ImageControl.Source = null;
                }
            }
        }

        private static void OnLayerNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageButton imageButton)
            {
                imageButton.LabelControl.Content = e.NewValue;
            }
        }


        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonClick?.Invoke(this, e);
        }

        public void SetSize(double width, double height)
        {
            ButtonControl.Width = width;
            ButtonControl.Height = height;
        }
        public void SetLayerNumber(int layerNumber)
        {
            LayerNumber = layerNumber;
        }
    }
}
