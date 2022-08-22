namespace ImageConvolver
{
    public class Invert : ITransform
    {
        public Invert()
        {
        }

        public PixelArray Transform(PixelArray input)
        {
            var data = Compute(input.PixelData);
            return new PixelArray(input.Stride, input.Format, data);
        }

        private byte[] Compute(byte[] input)
        {
            byte[] output = new byte[input.Length];

            for (int i = 0; i < input.Length; ++i)
            {
                output[i] = (byte)(0xff - input[i]);
            }

            return output;
        }
    }
}
