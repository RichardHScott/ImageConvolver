using System;
using System.Threading.Tasks;

namespace ImageConvolver
{
    public class GaussianBlur : ITransform
    {
        private readonly int kernelDivisor = 159;

        private readonly int[,] kernel =
        {
            { 2, 4, 5, 4, 2 },
            { 4, 9, 12, 9, 4 },
            { 5, 12, 15, 12, 5 },
            { 4, 9, 12, 9, 4 },
            { 2, 4, 5, 4, 2 }
        };

        public GaussianBlur()
        {
        }

        public PixelArray Transform(PixelArray input)
        {
            var channels = input.Format.BitsPerPixel / 8;
            var data = Blur(input.Stride, channels, input.PixelData);
            return new PixelArray(input.Stride, input.Format, data);
        }

        private byte[] Blur(int stride, int channels, byte[] input)
        {
            byte[] output = new byte[input.Length];

            int height = input.Length / stride;
            int kernelOffset = kernel.GetLength(0) / 2;
            int start = kernelOffset;
            int end = height - 2 * start;

            Parallel.For(start, end, i =>
            //for (int i = start; i < end; ++i)
            {
                var lineStart = kernelOffset * channels + i * stride;
                var lineEnd = (i + 1) * stride - kernelOffset * channels;
                for (int j = lineStart; j < lineEnd; j += channels)
                {
                    for (int cell = 0; cell < channels; ++cell)
                    {
                        int total = 0;
                        for (int ki = -kernelOffset; ki <= kernelOffset; ++ki)
                        {
                            for (int kj = -kernelOffset; kj <= kernelOffset; ++kj)
                            {
                                var pos = (ki * stride) + j + cell + channels * kj;
                                total += kernel[ki + kernelOffset, kj + kernelOffset] * input[pos];
                            }
                        }
                        output[j + cell] = (byte)(total / kernelDivisor);
                    }
                }
            });

            return output;
        }
    }
}
