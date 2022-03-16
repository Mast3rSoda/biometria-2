using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace biometria_2;

public static class Algorithm
{
    public static Bitmap Histogram(int width, int height, int[] histogram)
    {

        var bmpData = new byte[width * 3 * height];

        for (int i = 0; i < bmpData.Length; i++)
            bmpData[i] = 255;
        for (int i = 0; i < histogram.Length; i++)
        {
            for (int j = 0; j < histogram[i]; j++)
            {
                int index = i * 3 + (height - 1 - j) * width*3;

                bmpData[index + 0] =
                bmpData[index + 1] =
                bmpData[index + 2] = 0;
            }
        }

        Bitmap data = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        BitmapData bmpD = data.LockBits(
                       new Rectangle(0, 0, data.Width, data.Height),
                       ImageLockMode.ReadWrite, data.PixelFormat);
        Marshal.Copy(bmpData, 0, bmpD.Scan0, bmpData.Length);
        data.UnlockBits(bmpD);

        return data;
    }

    public static int[] getHistogramData(Bitmap bmp)
    {
        var data = bmp.LockBits(
            new Rectangle(0, 0, bmp.Width, bmp.Height),
            System.Drawing.Imaging.ImageLockMode.ReadWrite,
            System.Drawing.Imaging.PixelFormat.Format24bppRgb
        );
        var bmpData = new byte[data.Stride * data.Height];

        Marshal.Copy(data.Scan0, bmpData, 0, bmpData.Length);
        // Przerzuci z Bitmapy do tablicy

        int[] histogram = new int[256];
        foreach (byte i in bmpData)
            ++histogram[i];
        double max = histogram.Max();
        for (int i = 0; i < histogram.Length; i++)
            histogram[i] = (int)(histogram[i] / max * (double)data.Height);
        bmp.UnlockBits(data);
        return histogram;
    }

    public static int[] calculateLUT(int[] values)
    {
        //poszukaj wartości minimalnej
        int minValue = 0;
        for (int i = 0; i < 256; i++)
        {
            if (values[i] != 0)
            {
                minValue = i;
                break;
            }
        }

        //poszukaj wartości maksymalnej
        int maxValue = 255;
        for (int i = 255; i >= 0; i--)
        {
            if (values[i] != 0)
            {
                maxValue = i;
                break;
            }
        }

        //przygotuj tablice zgodnie ze wzorem
        int[] result = new int[256];
        double a = 255.0 / (maxValue - minValue);
        for (int i = 0; i < 256; i++)
        {
            result[i] = (int)(a * (i - minValue));
        }

        return result;
    }

    public static Bitmap StretchedHistogram(Bitmap bmp, int[] LUT)
    {
        Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);
        for (int x = 0; x < bmp.Width; x++)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                Color pixel = bmp.GetPixel(x, y);
                Color newPixel = Color.FromArgb(LUT[(pixel.R + pixel.G + pixel.B) / 3]);
                newBmp.SetPixel(x, y, newPixel);
            }
        }
        return Histogram(newBmp.Width, newBmp.Height, getHistogramData(newBmp));
    }
}
