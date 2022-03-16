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
        int[]? histogramValues = null;
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
                this.sourceImage = new Bitmap($"{fileName}");
                OriginalImage.Source = ImageSourceFromBitmap(this.sourceImage);
                histogramValues = Algorithm.getHistogramData(new Bitmap($"{fileName}"));
                HistogramImage.Source = ImageSourceFromBitmap(Algorithm.Histogram(this.sourceImage.Width, this.sourceImage.Height, histogramValues));
                int[] LUT = Algorithm.calculateLUT(histogramValues);
                StretchedHistogram.Source = ImageSourceFromBitmap(Algorithm.StretchedHistogram(new Bitmap($"{fileName}"), LUT));

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
    }

}
