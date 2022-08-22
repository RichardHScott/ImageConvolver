using System.Windows.Media;

namespace ImageConvolver
{
    public class PixelArray
    {
        public PixelFormat Format { get; }
        public int Stride { get; }
        public byte[] PixelData { get; }

        public PixelArray(int stride, PixelFormat format, byte[] pixelData)
        {
            Stride = stride;
            Format = format;
            PixelData = pixelData;
        }
    }
}
