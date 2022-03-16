using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
                int index = i * 3 + (height - 1 - j) * width * 3;

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


    public static double[][] getHistogramData(Bitmap bmp, WpfPlot? histPlot)
    {
        var data = bmp.LockBits(
            new Rectangle(0, 0, bmp.Width, bmp.Height),
            System.Drawing.Imaging.ImageLockMode.ReadWrite,
            System.Drawing.Imaging.PixelFormat.Format24bppRgb
        );
        var bmpData = new byte[data.Stride * data.Height];

        Marshal.Copy(data.Scan0, bmpData, 0, bmpData.Length);
        // Przerzuci z Bitmapy do tablicy

        double[] histogramB = new double[256];
        double[] histogramR = new double[256];
        double[] histogramG = new double[256];
        double[] histogram = new double[256];


        for (int i = 0; i < bmpData.Length; i++)
        {
            ++histogram[bmpData[i]];
            if ((i) % 3 == 0)
                ++histogramB[bmpData[i]];
            if ((i + 2) % 3 == 0)
                ++histogramG[bmpData[i]];
            if ((i + 1) % 3 == 0)
                ++histogramR[bmpData[i]];
        }
        double max = histogram.Max();
        for (int i = 0; i < histogram.Length; i++)
        {
            histogram[i] = (double)(histogram[i] / max * (double)data.Height);
            histogramB[i] = (double)(histogramB[i] / max * (double)data.Height);
            histogramR[i] = (double)(histogramR[i] / max * (double)data.Height);
            histogramG[i] = (double)(histogramG[i] / max * (double)data.Height);

        }


        double[] xRow = new double[histogram.Length];
        for (int i = 0; i < histogram.Length; i++)
        {
            xRow[i] = i;
        }
        bmp.UnlockBits(data);

        if (histPlot == null)
            return new double[][] { histogram, histogramB, histogramG, histogramR };

        histPlot.Reset();
        histPlot.Plot.AddScatter(xRow, histogram, Color.Black);
        histPlot.Plot.AddScatter(xRow, histogramB, Color.Blue);
        histPlot.Plot.AddScatter(xRow, histogramR, Color.Red);
        histPlot.Plot.AddScatter(xRow, histogramG, Color.Green);

        histPlot.Refresh();
        
        return new double[][] { histogram, histogramB, histogramG, histogramR };
    }

    #region wyrownanie histogramu
    private static int[] CalculateEqualLUT(double[] values, int size)
    {
        //to jest zjebane w chuj, tutaj bym szukał powodyra.... albo źle zrobiłem histogramy
        double minValue = 0;
        for (int i = 0; i < 256; i++)
        {
            if (values[i] != 0)
            {
                minValue = values[i];
                break;
            }
        }

        int[] result = new int[256];
        int sum = 0;
        for (int i = 0; i < 256; i++)
        {
            sum += (int)values[i];
            result[i] = (int)((sum - minValue) / (size - minValue) * 255 *5)%255;//daje dobre efekty dla * size/500 zamiast 255, ale tylko dla czarnobiałych obrazów (normalnie jest 255.0)
        }

        return result;


    }

    public static Bitmap EqualizeHistogram(Bitmap bitmap, WpfPlot histPlost)
    {
        double[][] histogramsData = getHistogramData(bitmap, null);
        int[] LUTblue = CalculateEqualLUT(histogramsData[1], bitmap.Width * bitmap.Height);
        int[] LUTgreen = CalculateEqualLUT(histogramsData[2], bitmap.Width * bitmap.Height);
        int[] LUTred = CalculateEqualLUT(histogramsData[3], bitmap.Width * bitmap.Height);


        unsafe
        {
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

            int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;
            byte* PtrFirstPixel = (byte*)bitmapData.Scan0;


            Parallel.For(0, heightInPixels, y =>
            {
                byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    currentLine[x] = (byte)LUTblue[currentLine[x]];
                    currentLine[x + 1] = (byte)LUTgreen[currentLine[x + 1]];
                    currentLine[x + 2] = (byte)LUTred[currentLine[x + 2]];
                }
            });
            bitmap.UnlockBits(bitmapData);
            _ = getHistogramData(bitmap, histPlost);
            return bitmap;
            for (int y = 0; y < heightInPixels; y++)
            {
                byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    currentLine[x] = (byte)LUTblue[currentLine[x]];
                    currentLine[x + 1] = (byte)LUTgreen[currentLine[x + 1]];
                    currentLine[x + 2] = (byte)LUTred[currentLine[x + 2]];
                }
            }
        }
        //gówno.... tak było oryginalnie
        Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                Color pixel = bitmap.GetPixel(x, y);
                Color newPixel = Color.FromArgb(LUTred[pixel.R], LUTgreen[pixel.G], LUTblue[pixel.B]);
                newBitmap.SetPixel(x, y, newPixel);
            }
        }
        return newBitmap;

    }

    #endregion
    public static Bitmap AdjustBrightness(Bitmap Image, int Value)
    {
        System.Drawing.Bitmap TempBitmap = Image;
        float FinalValue = (float)Value / 255.0f;
        System.Drawing.Bitmap NewBitmap = new System.Drawing.Bitmap(TempBitmap.Width, TempBitmap.Height);
        System.Drawing.Graphics NewGraphics = System.Drawing.Graphics.FromImage(NewBitmap);
        float[][] FloatColorMatrix ={
                     new float[] {1, 0, 0, 0, 0},
                     new float[] {0, 1, 0, 0, 0},
                     new float[] {0, 0, 1, 0, 0},
                     new float[] {0, 0, 0, 1, 0},
                     new float[] {FinalValue, FinalValue, FinalValue, 1, 1}
                 };
        System.Drawing.Imaging.ColorMatrix NewColorMatrix = new ColorMatrix(FloatColorMatrix);
        System.Drawing.Imaging.ImageAttributes Attributes = new ImageAttributes();
        Attributes.SetColorMatrix(NewColorMatrix);
        NewGraphics.DrawImage(TempBitmap, new System.Drawing.Rectangle(0, 0, TempBitmap.Width, TempBitmap.Height), 0, 0, TempBitmap.Width, TempBitmap.Height, System.Drawing.GraphicsUnit.Pixel, Attributes);
        Attributes.Dispose();
        NewGraphics.Dispose();
        return NewBitmap;
    }

    public static int[] calculateLUT(double[] values, int thresholdValue, int minval)
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
        double a = 255.0 / (thresholdValue - minval);
        for (int i = 0; i < 256; i++)
        {
            result[i] = (int)(a * (i - minval));
        }

        return result;
    }

    public static Bitmap StretchedHistogram(Bitmap bmp, WpfPlot plot, int value=256, int value2=0)
    {
        double[][] histogramsData = getHistogramData(bmp, null);
        int[] LUTblue = calculateLUT(histogramsData[1],value, value2);
        int[] LUTgreen = calculateLUT(histogramsData[2],value, value2);
        int[] LUTred = calculateLUT(histogramsData[3], value, value2);

        //Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);
        //for (int x = 0; x < bmp.Width; x++)
        //{
        //    for (int y = 0; y < bmp.Height; y++)
        //    {
        //        Color pixel = bmp.GetPixel(x, y);
        //        Color newPixel = Color.FromArgb(LUTred[pixel.R], LUTgreen[pixel.G], LUTblue[pixel.B]);
        //        newBmp.SetPixel(x, y, newPixel);
        //    }
        //}
        unsafe
        {
            BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);

            int bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;
            byte* PtrFirstPixel = (byte*)bitmapData.Scan0;


            Parallel.For(0, heightInPixels, y =>
            {
                byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    currentLine[x] = (byte)LUTblue[currentLine[x]];
                    currentLine[x + 1] = (byte)LUTgreen[currentLine[x + 1]];
                    currentLine[x + 2] = (byte)LUTred[currentLine[x + 2]];
                }
            });
            bmp.UnlockBits(bitmapData);
            _ = getHistogramData(bmp, plot);
            return bmp;
            /*for (int y = 0; y < heightInPixels; y++)
            {
                byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    currentLine[x] = (byte)LUTblue[currentLine[x]];
                    currentLine[x + 1] = (byte)LUTgreen[currentLine[x + 1]];
                    currentLine[x + 2] = (byte)LUTred[currentLine[x + 2]];
                }
            }*/
        }
    }

    public static Bitmap GetOtsu(Bitmap bmp, WpfPlot plot)
    {
        double[] histogram = getHistogramData(bmp, null)[0];

        double avgValue = 0;

        for (int i = 0; i < 256; i++)
        {
            avgValue += histogram[i];
        }

        avgValue /= histogram.Length;


        ////Global mean
        //double mg = 0;
        //for (int i = 0; i < 255; i++)
        //{
        //    mg += i * histogram[i];
        //}

        ////Get max between-class variance
        //double bcv = 0;
        //int threshold = 0;
        //for (int i = 0; i < 256; i++)
        //{
        //    double cs = 0;
        //    double m = 0;
        //    for (int j = 0; j < i; j++)
        //    {
        //        cs += histogram[j];
        //        m += j * histogram[j];
        //    }

        //    if (cs == 0)
        //    {
        //        continue;
        //    }

        //    double old_bcv = bcv;
        //    bcv = Math.Max(bcv, Math.Pow(mg * cs - m, 2) / (cs * (1 - cs)));

        //    if (bcv > old_bcv)
        //    {
        //        threshold = i;
        //    }
        //}


        unsafe
        {
            BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);

            int bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;
            byte* PtrFirstPixel = (byte*)bitmapData.Scan0;


            Parallel.For(0, heightInPixels, y =>
            {
                byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    if((currentLine[x] + currentLine[x + 1] + currentLine[x + 2]/3) > avgValue)
                        currentLine[x] = currentLine[x + 1] = currentLine[x + 2] = byte.MaxValue;
                    else
                        currentLine[x] = currentLine[x + 1] = currentLine[x + 2] = byte.MinValue;


                }
            });
            bmp.UnlockBits(bitmapData);
            _ = getHistogramData(bmp, plot);

            return bmp;
        }
    }
}