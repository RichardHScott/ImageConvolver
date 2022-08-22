using System;

namespace ImageConvolver
{
    public class Sobel : ITransform
    {
        private readonly int[,] kernelX =
        {
            { 1, 0, -1 },
            { 2, 0, -2 },
            { 1, 0, -1 },
        };

        private readonly int[,] kernelY =
        {
            { 1, 2, 1 },
            { 0, 0, -0 },
            { -1, -2, -1 },
        };

        public Sobel()
        {
        }

        public PixelArray Transform(PixelArray input)
        {
            var data = FindGradients(input.PixelData, input.Stride);
            return new PixelArray(input.Stride, input.Format, data);
        }

        public byte[] FindGradients(byte[] input, int stride)
        {

            byte[] output = new byte[input.Length];

            int height = input.Length / stride;
            int kernelOffset = kernelX.GetLength(0) / 2;
            int start = kernelOffset;
            int end = height - 2 * start;

            for (int i = start; i < end; ++i)
            {
                var lineStart = kernelOffset + i * stride;
                var lineEnd = (i + 1) * stride - kernelOffset;
                for (int j = lineStart; j < lineEnd; j += 1)
                {
                    int totalX = 0;
                    int totalY = 0;
                    for (int ki = -kernelOffset; ki <= kernelOffset; ++ki)
                    {
                        for (int kj = -kernelOffset; kj <= kernelOffset; ++kj)
                        {
                            var pos = (ki * stride) + j + kj;
                            totalX += kernelX[ki + kernelOffset, kj + kernelOffset] * input[pos];
                            totalY += kernelY[ki + kernelOffset, kj + kernelOffset] * input[pos];
                        }
                    }
                    output[j] = (byte)Math.Sqrt(totalX * totalX + totalY * totalY);
                }
            }

            return output;
        }
    }
}
