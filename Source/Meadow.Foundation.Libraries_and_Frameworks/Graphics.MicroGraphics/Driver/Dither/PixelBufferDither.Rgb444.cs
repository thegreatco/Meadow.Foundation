using Meadow;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;
using System;

namespace Graphics.MicroGraphics.Dither
{
    public static partial class PixelBufferDither
    {
        private static readonly byte[,] BAYER4_12bpp = new byte[,]
        {
            { 0, 8, 2, 10 },
            { 12, 4, 14, 6 },
            { 3, 11, 1, 9 },
            { 15, 7, 13, 5 }
        };

        /// <summary>
        /// Convert any IPixelBuffer to a new BufferRgb444 with optional dithering.
        /// </summary>
        public static BufferRgb444 ToRgb444(
            IPixelBuffer sourceBuffer,
            DitherMode mode,
            bool serpentine = true)
        {
            if (sourceBuffer is null)
            {
                throw new ArgumentNullException(nameof(sourceBuffer));
            }

            var ditheredBuffer = new BufferRgb444(sourceBuffer.Width, sourceBuffer.Height);

            switch (mode)
            {
                case DitherMode.Ordered4x4:
                    ConvertOrdered4x4_Rgb444(sourceBuffer, ditheredBuffer);
                    break;
                case DitherMode.FloydSteinberg:
                    ConvertFloydSteinberg_Rgb444(sourceBuffer, ditheredBuffer, serpentine);
                    break;
            }

            return ditheredBuffer;
        }

        private static void ConvertOrdered4x4_Rgb444(IPixelBuffer sourceBuffer, BufferRgb444 destinationBuffer)
        {
            for (int y = 0; y < destinationBuffer.Height; y++)
            {
                int yMatrixIndex = y & 3;
                for (int x = 0; x < destinationBuffer.Width; x++)
                {
                    var sourceColor = sourceBuffer.GetPixel(x, y);

                    // Normalize the dither threshold to the 8-bit range
                    int threshold = BAYER4_12bpp[yMatrixIndex, x & 3] * 16;

                    // Get the lower 4 bits (quantization error)
                    int rRemainder = sourceColor.R & 0x0F;
                    int gRemainder = sourceColor.G & 0x0F;
                    int bRemainder = sourceColor.B & 0x0F;

                    // Calculate the 4-bit quantized color
                    int r4 = sourceColor.R >> 4;
                    int g4 = sourceColor.G >> 4;
                    int b4 = sourceColor.B >> 4;

                    // Apply dithering by adding 1 if the remainder is greater than the threshold
                    if (rRemainder > threshold)
                    {
                        r4++;
                    }
                    if (gRemainder > threshold)
                    {
                        g4++;
                    }
                    if (bRemainder > threshold)
                    {
                        b4++;
                    }

                    // Convert the dithered 4-bit values back to 8-bit for the destination buffer
                    byte finalR = (byte)(Math.Min(r4, 15) << 4);
                    byte finalG = (byte)(Math.Min(g4, 15) << 4);
                    byte finalB = (byte)(Math.Min(b4, 15) << 4);

                    destinationBuffer.SetPixel(x, y, new Color(finalR, finalG, finalB));
                }
            }
        }

        private static void ConvertFloydSteinberg_Rgb444(IPixelBuffer sourceBuffer, BufferRgb444 destinationBuffer, bool serpentine)
        {
            int width = destinationBuffer.Width;
            int height = destinationBuffer.Height;

            var currentRowErrorR = new int[width + 2];
            var currentRowErrorG = new int[width + 2];
            var currentRowErrorB = new int[width + 2];
            var nextRowErrorR = new int[width + 2];
            var nextRowErrorG = new int[width + 2];
            var nextRowErrorB = new int[width + 2];

            for (int y = 0; y < height; y++)
            {
                if (!serpentine || (y & 1) == 0)
                {
                    // Left to right
                    for (int x = 0; x < width; x++)
                    {
                        DitherPixelAndDiffuseError(x, y, 1);
                    }
                }
                else
                {
                    // Right to left
                    for (int x = width - 1; x >= 0; x--)
                    {
                        DitherPixelAndDiffuseError(x, y, -1);
                    }
                }

                // Swap the row accumulators
                (currentRowErrorR, nextRowErrorR) = (nextRowErrorR, currentRowErrorR);
                (currentRowErrorG, nextRowErrorG) = (nextRowErrorG, currentRowErrorG);
                (currentRowErrorB, nextRowErrorB) = (nextRowErrorB, currentRowErrorB);

                // Clear the new `nextRowError` arrays for the next scanline's diffusion
                Array.Clear(nextRowErrorR, 0, nextRowErrorR.Length);
                Array.Clear(nextRowErrorG, 0, nextRowErrorG.Length);
                Array.Clear(nextRowErrorB, 0, nextRowErrorB.Length);
            }

            void DitherPixelAndDiffuseError(int x, int y, int direction)
            {
                int errorArrayIndex = x + 1;
                var sourceColor = sourceBuffer.GetPixel(x, y);

                // Add the accumulated error from previous pixels
                int adjustedR = ClampByte(sourceColor.R + (currentRowErrorR[errorArrayIndex] >> 8));
                int adjustedG = ClampByte(sourceColor.G + (currentRowErrorG[errorArrayIndex] >> 8));
                int adjustedB = ClampByte(sourceColor.B + (currentRowErrorB[errorArrayIndex] >> 8));

                // Quantize to the 4-bit palette
                byte quantizedR = (byte)((adjustedR & 0xF0) | (adjustedR >> 4));
                byte quantizedG = (byte)((adjustedG & 0xF0) | (adjustedG >> 4));
                byte quantizedB = (byte)((adjustedB & 0xF0) | (adjustedB >> 4));

                destinationBuffer.SetPixel(x, y, new Color(quantizedR, quantizedG, quantizedB));
                var finalQuantizedColor = destinationBuffer.GetPixel(x, y);

                // Calculate the quantization error and scale it to fixed-point (Q8.8)
                int errorR = (adjustedR - finalQuantizedColor.R);
                int errorG = (adjustedG - finalQuantizedColor.G);
                int errorB = (adjustedB - finalQuantizedColor.B);

                // Distribute the error using Floyd-Steinberg coefficients
                // Note: The coefficients (7, 3, 5, 1) sum to 16.
                // We use multiplication by 7, 3, 5, 1 and then division by 16 (>> 4)
                if (direction > 0)
                {
                    // Forward pass
                    currentRowErrorR[errorArrayIndex + 1] += (7 * errorR);
                    currentRowErrorG[errorArrayIndex + 1] += (7 * errorG);
                    currentRowErrorB[errorArrayIndex + 1] += (7 * errorB);

                    nextRowErrorR[errorArrayIndex - 1] += (3 * errorR);
                    nextRowErrorG[errorArrayIndex - 1] += (3 * errorG);
                    nextRowErrorB[errorArrayIndex - 1] += (3 * errorB);

                    nextRowErrorR[errorArrayIndex] += (5 * errorR);
                    nextRowErrorG[errorArrayIndex] += (5 * errorG);
                    nextRowErrorB[errorArrayIndex] += (5 * errorB);

                    nextRowErrorR[errorArrayIndex + 1] += (1 * errorR);
                    nextRowErrorG[errorArrayIndex + 1] += (1 * errorG);
                    nextRowErrorB[errorArrayIndex + 1] += (1 * errorB);
                }
                else
                {
                    // Reverse pass
                    currentRowErrorR[errorArrayIndex - 1] += (7 * errorR);
                    currentRowErrorG[errorArrayIndex - 1] += (7 * errorG);
                    currentRowErrorB[errorArrayIndex - 1] += (7 * errorB);

                    nextRowErrorR[errorArrayIndex + 1] += (3 * errorR);
                    nextRowErrorG[errorArrayIndex + 1] += (3 * errorG);
                    nextRowErrorB[errorArrayIndex + 1] += (3 * errorB);

                    nextRowErrorR[errorArrayIndex] += (5 * errorR);
                    nextRowErrorG[errorArrayIndex] += (5 * errorG);
                    nextRowErrorB[errorArrayIndex] += (5 * errorB);

                    nextRowErrorR[errorArrayIndex - 1] += (1 * errorR);
                    nextRowErrorG[errorArrayIndex - 1] += (1 * errorG);
                    nextRowErrorB[errorArrayIndex - 1] += (1 * errorB);
                }
            }
        }
    }
}