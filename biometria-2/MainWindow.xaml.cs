using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace biometria_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Bitmap? sourceImage = null;
        Bitmap? imageToEdit = null;
        double[]? histogramValues = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg;*.png)|*.jpg;*.png|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                imageToEdit = this.sourceImage = new Bitmap($"{fileName}");
                OriginalImage.Source = ImageSourceFromBitmap(this.sourceImage);
               /* histogramValues = Algorithm.getHistogramData(new Bitmap($"{fileName}"));
                HistogramImage.Source = ImageSourceFromBitmap(Algorithm.Histogram(this.sourceImage.Width, this.sourceImage.Height, histogramValues));
                int[] LUT = Algorithm.calculateLUT(histogramValues);
                StretchedHistogram.Source = ImageSourceFromBitmap(Algorithm.StretchedHistogram(new Bitmap($"{fileName}"), LUT));*/ //sorki najmocniej, będziesz musiał poscalać z powrotem

                _= Algorithm.getHistogramData(new Bitmap($"{fileName}"), histPlot);
                //HistogramImage.Source = ImageSourceFromBitmap(Algorithm.Histogram(new Bitmap($"{fileName}"), histogramValues));
                
            }
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }


        private void Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EqualHistogram(object sender, RoutedEventArgs e)
        {
            if (sourceImage == null)
            {
                MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Bitmap bitmap = new Bitmap(this.sourceImage.Width, this.sourceImage.Height);
            bitmap = (Bitmap)this.imageToEdit.Clone();
            newImage.Source = ImageSourceFromBitmap(Algorithm.EqualizeHistogram( bitmap, newHistPlot));
        }

        private void StretchedHistogram(object sender, RoutedEventArgs e)
        {
            if (sourceImage == null)
            {
                MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Bitmap bitmap = new Bitmap(this.sourceImage.Width, this.sourceImage.Height);
            bitmap = (Bitmap)this.imageToEdit.Clone();
            newImage.Source = ImageSourceFromBitmap(Algorithm.StretchedHistogram(bitmap, newHistPlot));
        }

        private void BrightnessValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sourceImage == null)
            {
                MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            imageToEdit = Algorithm.AdjustBrightness(this.sourceImage, (int)BrightnessValue.Value);
            OriginalImage.Source = ImageSourceFromBitmap(imageToEdit);
            _ = Algorithm.getHistogramData(imageToEdit, histPlot);
        }

        private void Otsu(object sender, RoutedEventArgs e)
        {
            if (sourceImage == null)
            {
                MessageBox.Show("You haven't uploaded any files", "Image error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
    }

}
