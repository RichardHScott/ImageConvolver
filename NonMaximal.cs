using System;
using System.Threading.Tasks;

namespace ImageConvolver
{
    public class NonMaximal : ITransform
    {
        private readonly int weakThreshold;
        private readonly int strongThreshold;

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

        private const int kernelOffset = 1; // kernelX.GetLength(0) / 2;

        public NonMaximal(int weakThreshold, int strongThreshold)
        {
            this.weakThreshold = weakThreshold;
            this.strongThreshold = strongThreshold;
        }

        public PixelArray Transform(PixelArray input)
        {
            var data = Compute(input.PixelData, input.Stride);
            return new PixelArray(input.Stride, input.Format, data);
        }

        private byte[] Compute(byte[] input, int stride)
        {
            byte[] dirGrads = new byte[input.Length];
            var dirTheta = new (short, short)[input.Length];

            int height = input.Length / stride;
            int start = kernelOffset;
            int end = height - 2 * start;

            ForEachPixel(input, stride, j =>
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

                dirGrads[j] = (byte)Math.Sqrt(totalX * totalX + totalY * totalY);
                dirTheta[j] = Theta(totalX, totalY);
            });

            byte[] output = new byte[input.Length];

            ForEachPixel(dirGrads, stride, j =>
            {
                var dir = dirTheta[j];
                var pos1 = (dir.Item1 * stride) + j + dir.Item2;
                var pos2 = (-dir.Item1 * stride) + j - dir.Item2;
                var max = Math.Max(dirGrads[pos1], dirGrads[pos2]);

                output[j] = dirGrads[j] < max ? (byte)0x0 : dirGrads[j];
            });

            byte[] output2 = new byte[input.Length];

            var testDirs = new (short, short)[] {
                (-1, -1), (-1, 0), (-1, 1),
                (0, -1), (0, 1),
                (1, -1), (1, 0), (1, 1),
            };

            ForEachPixel(output, stride, j =>
            {
                if (output[j] < weakThreshold)
                {
                    output2[j] = 0;
                }
                else if (output[j] > strongThreshold)
                {
                    output2[j] = input[j];
                }
                else
                {
                    // Is it 8 connected?
                    foreach (var dir in testDirs)
                    {
                        var pos = (dir.Item1 * stride) + j + dir.Item2;
                        var neighbour = output[pos];
                        if (neighbour > strongThreshold)
                        {
                            output2[j] = input[j];
                            return;
                        }
                    }
                    output2[j] = 0;
                }
            });

            return output2;
        }

        private static (short, short) Theta(int totalX, int totalY)
        {
            var theta = Math.Atan2(totalY, totalX);

            if (0 <= theta && theta <= 22.5)
            {
                return (0, 1);
            }
            else if (22.5 <= theta && theta <= 67.5)
            {
                return (1, 1);
            }
            else if (67.5 <= theta && theta <= 112.5)
            {
                return (1, 0);
            }
            else if (112.5 <= theta && theta <= 157.5)
            {
                return (1, -1);
            }
            else // -ve x direction
            {
                return (0, 1);
            }
        }

        private static void ForEachPixel(byte[] input, int stride, Action<int> callback)
        {
            int height = input.Length / stride;
            int start = kernelOffset;
            int end = height - 2 * start;

            Parallel.For(start, end, i =>
            {
                var lineStart = kernelOffset + i * stride;
                var lineEnd = (i + 1) * stride - kernelOffset;

                for (int j = lineStart; j < lineEnd; j += 1)
                {
                    callback(j);
                }
            });

            //for (int i = start; i < end; ++i)
            //{
            //    var lineStart = kernelOffset + i * stride;
            //    var lineEnd = (i + 1) * stride - kernelOffset;

            //    for (int j = lineStart; j < lineEnd; j += 1)
            //    {
            //        callback(j);
            //    }
            //}
        }
    }
}
