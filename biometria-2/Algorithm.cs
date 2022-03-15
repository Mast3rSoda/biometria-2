using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace biometria_2;

public static class Algorithm
{
    public static Bitmap Histogram(Bitmap bmp, int[] histogram)
    {
        var data = bmp.LockBits(
            new Rectangle(0, 0, bmp.Width, bmp.Height),
            System.Drawing.Imaging.ImageLockMode.ReadWrite,
            System.Drawing.Imaging.PixelFormat.Format24bppRgb
        );
        var bmpData = new byte[data.Stride * data.Height];
        for (int i = 0; i < bmpData.Length; i++)
            bmpData[i] = 255;
        for (int i = 0; i < histogram.Length; i++)
        {
            for (int j = 0; j < histogram[i]; j++)
            {
                int index = i * 3 + (data.Height - 1 - j) * data.Stride;

                bmpData[index + 0] =
                bmpData[index + 1] =
                bmpData[index + 2] = 0;
            }
        }

        Marshal.Copy(bmpData, 0, data.Scan0, bmpData.Length);
        // Przerzuci z tablicy do Bitmapy

        bmp.UnlockBits(data);
        return bmp;
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

        return histogram;
    }
}
