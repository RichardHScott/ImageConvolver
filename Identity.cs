namespace ImageConvolver
{
    internal class Identity : ITransform
    {
        public PixelArray Transform(PixelArray input)
        {
            return input;
        }
    }
}