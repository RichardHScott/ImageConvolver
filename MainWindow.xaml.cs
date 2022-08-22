using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageConvolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ImageData imageData;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "Images|*.jpg;*.jpeg;*.bmp;*.png"
                + "|All Files|*.*";

            if (dialog.ShowDialog() == true)
            {
                OpenFile(dialog.FileName);
            }
        }

        private void OpenFile(string fileName)
        {
            imageData = new ImageData(fileName);
            originalImage.Source = imageData.OriginalImage;

            SetOutput();
        }

        private void SetOutput()
        {
            computedImage.Source = imageData.ComputedImage;
            TimeTaken.Text = $"{imageData.TimeTakenMS} ms";
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = ((CheckBox)sender).IsChecked == true;
            if (isChecked)
            {
                var transform = new CompositeTransform(new List<ITransform>()
                {
                    new GaussianBlur(),
                    new NonMaximal(25, 75)
                });
                imageData.Transform = transform;
                imageData.Refresh();
            } else
            {
                if (!int.TryParse(lowPassFilterSize.Text, out var size))
                {
                    lowPassFilterSize.Text = "Filter size must be odd number. Setting to 9.";
                    size = 9;
                }

                if (size % 2 != 1)
                {
                    lowPassFilterSize.Text = "Filter size must be odd number. Setting to 9.";
                    size = 9;
                }

                imageData.Transform = new LowPass(size);
                imageData.Refresh();
            }
            SetOutput();
        }
    }
}
