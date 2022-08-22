using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageConvolver
{
    public class ImageData
    {
        public string FileName { get; }
        public BitmapImage OriginalImage { get; }
        public BitmapImage ComputedImage { get; private set; }

        private PixelArray greyscaleData;

        public ITransform Transform { get; set; }

        public string TimeTakenMS { get; set; }

        public ImageData(string fileName)
        {
            FileName = fileName;
            OriginalImage = new BitmapImage(new Uri(fileName));

            var width = OriginalImage.PixelWidth;
            var height = OriginalImage.PixelHeight;

            var format = OriginalImage.Format;
            var bytesPerPixel = (format.BitsPerPixel / 8);
            var stride = bytesPerPixel * width;
            var dest = new byte[stride * height];

            OriginalImage.CopyPixels(dest, stride, 0);

            greyscaleData = ConvertToGreyScale(new PixelArray(stride, OriginalImage.Format, dest));

            Transform = new CompositeTransform(new List<ITransform>()
            {
                new GaussianBlur(),
                new NonMaximal(25, 75)
            });
            UpdateBitmap();
        }

        public void Refresh()
        {
            UpdateBitmap();
        }

        private void UpdateBitmap()
        {
            var sw = new Stopwatch();
            sw.Start();
            var newData = Transform.Transform(greyscaleData);
            sw.Stop();
            TimeTakenMS = $"{sw.ElapsedMilliseconds}";

            var wb = new WriteableBitmap(OriginalImage.PixelWidth, OriginalImage.PixelHeight, OriginalImage.DpiX, OriginalImage.DpiY, newData.Format, null);
            wb.WritePixels(new Int32Rect(0, 0, OriginalImage.PixelWidth, OriginalImage.PixelHeight), newData.PixelData, newData.Stride, 0);
            ComputedImage = wb.ToBitmapImage();
        }

        // Only 4 channel, 1 full of 0xff supported i.e. RGBA with alpha 1.0
        private static PixelArray ConvertToGreyScale(PixelArray inputData)
        {
            var input = inputData.PixelData;
            var channels = 3;

            byte[] output = new byte[input.Length / channels];

            int i = 0, o = 0;
            while (i < input.Length)
            {
                int total = 0;
                for (int c=0; c < channels; c++)
                {
                    total += input[i++];
                }
                i++;
                output[o++] = (byte)(total / 3);
            }

            return new PixelArray(inputData.Stride / 4, PixelFormats.Gray8, output);
        }
    }
}
