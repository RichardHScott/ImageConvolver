using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageConvolver
{
    internal class CompositeTransform : ITransform
    {
        private readonly List<ITransform> transforms;

        public CompositeTransform(IEnumerable<ITransform> transforms)
        {
            if (transforms is null)
            {
                throw new ArgumentNullException(nameof(transforms));
            }

            if (!transforms.Any())
            {
                throw new ArgumentException("Need at least one transform");
            }

            this.transforms = new List<ITransform>(transforms);
        }

        public PixelArray Transform(PixelArray input)
        {
            PixelArray output = null;
            foreach (var xfrm in transforms)
            {
                output = xfrm.Transform(input);
            }
            return output;
        }
    }
}