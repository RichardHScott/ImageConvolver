using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageConvolver
{
    public class LowPass : ITransform
    {
        private readonly int kernelFactor = 9;
        private readonly int[,] kernel =
        {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 1, 1, 1 },
        };

        public LowPass(int size)
        {
            kernelFactor = size * size;
            kernel = new int[size, size];
            for (int i = 0; i < kernel.GetLength(0); i++)
                for (int j = 0; j < kernel.GetLength(1); j++)
                    kernel[i, j] = 1;
        }

        public PixelArray Transform(PixelArray input)
        {
            var data = Filter(input.PixelData, input.Stride);
            return new PixelArray(input.Stride, input.Format, data);
        }

        public byte[] Filter(byte[] input, int stride)
        {
            byte[] output = new byte[input.Length];

            int height = input.Length / stride;
            int kernelOffset = kernel.GetLength(0) / 2;
            int start = kernelOffset;
            int end = height - 2 * start;

            Parallel.For(start, end, i =>
            {
                var lineStart = kernelOffset + i * stride;
                var lineEnd = (i + 1) * stride - kernelOffset;
                for (int j = lineStart; j < lineEnd; j += 1)
                {
                    int totalX = 0;
                    for (int ki = -kernelOffset; ki <= kernelOffset; ++ki)
                    {
                        for (int kj = -kernelOffset; kj <= kernelOffset; ++kj)
                        {
                            var pos = (ki * stride) + j + kj;
                            totalX += kernel[ki + kernelOffset, kj + kernelOffset] * input[pos];
                        }
                    }
                    output[j] = (byte)(totalX / kernelFactor);
                }
            });

            return output;
        }
    }
}
