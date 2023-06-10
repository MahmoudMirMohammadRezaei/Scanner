namespace NAPS2.Images.Bitwise;

/// <summary>
/// Runs a bilateral filter operation, which reduces noise without losing edges or fine details.
/// https://en.wikipedia.org/wiki/Bilateral_filter
/// </summary>
public class BilateralFilterOp : BinaryBitwiseImageOp
{
    // The color distance (in the 0-255 range) at which pixels are weighted to 0.
    // The weight linearly scales up as the color distance approaches 0.
    private const int COLOR_DIST_MAX = 32;

    // The size of the square around the current pixel to check.
    // Should be an odd number for symmetry.
    // Pixels in the square are weighted by how close they are to the center.
    private const int FILTER_SIZE = 15;

    protected override LockMode SrcLockMode => LockMode.ReadOnly;
    protected override LockMode DstLockMode => LockMode.WriteOnly;

    protected override void PerformCore(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        // TODO: Implement grayscale?
        if (src.bytesPerPixel is 3 or 4 && dst.bytesPerPixel is 3 or 4)
        {
            PerformRgba(src, dst, partStart, partEnd);
        }
        else
        {
            throw new InvalidOperationException("Unsupported pixel format");
        }
    }

    private unsafe void PerformRgba(BitwiseImageData src, BitwiseImageData dst, int partStart, int partEnd)
    {
        bool copyAlpha = src.hasAlpha && dst.hasAlpha;
        const int s = FILTER_SIZE / 2;

        var filter = new int[FILTER_SIZE, FILTER_SIZE];
        for (int filterX = 0; filterX < FILTER_SIZE; filterX++)
        {
            for (int filterY = 0; filterY < FILTER_SIZE; filterY++)
            {
                int dx = filterX - s;
                int dy = filterY - s;
                var dmax = Math.Sqrt(2 * s * s);
                var d = Math.Sqrt(dx * dx + dy * dy) / dmax;
                filter[filterX, filterY] = (int)((1 - d) * 256);
            }
        }

        var diffWeights = new int[256 * 3 * 2];
        for (int i = 0; i < COLOR_DIST_MAX * 3; i++)
        {
            diffWeights[256 * 3 + i] = COLOR_DIST_MAX - i / 3;
            diffWeights[256 * 3 - i] = COLOR_DIST_MAX - i / 3;
        }

        for (int i = partStart; i < partEnd; i++)
        {
            var srcRow = src.ptr + src.stride * i;
            var dstRow = dst.ptr + dst.stride * i;
            for (int j = 0; j < src.w; j++)
            {
                var srcPixel = srcRow + j * src.bytesPerPixel;
                var dstPixel = dstRow + j * dst.bytesPerPixel;
                int r = *(srcPixel + src.rOff);
                int g = *(srcPixel + src.gOff);
                int b = *(srcPixel + src.bOff);

                if (j > s && j < src.w - s && i > s && i < src.h - s)
                {
                    bool skipPixel = false;
                    if (r == 255 && g == 255 & b == 255)
                    {
                        var prevPixel = src.ptr + src.stride * i + src.bytesPerPixel * j - 1;
                        var nextPixel = src.ptr + src.stride * i + src.bytesPerPixel * j + 1;
                        byte prevR = *(prevPixel + src.rOff);
                        byte prevG = *(prevPixel + src.gOff);
                        byte prevB = *(prevPixel + src.bOff);
                        byte nextR = *(nextPixel + src.rOff);
                        byte nextG = *(nextPixel + src.gOff);
                        byte nextB = *(nextPixel + src.bOff);
                        if (prevR == 255 && prevG == 255 && prevB == 255 && nextR == 255 && nextG == 255 && nextB == 255)
                        {
                            skipPixel = true;
                        }
                    }
                    if (!skipPixel)
                    {
                        int rTotal = 0, gTotal = 0, bTotal = 0;
                        int weightTotal = 0;
                        for (int filterX = 0; filterX < FILTER_SIZE; filterX++)
                        {
                            for (int filterY = 0; filterY < FILTER_SIZE; filterY++)
                            {
                                int imageX = j - s + filterX;
                                int imageY = i - s + filterY;

                                var pixel = src.ptr + src.stride * imageY + src.bytesPerPixel * imageX;

                                var r2 = *(pixel + src.rOff);
                                var g2 = *(pixel + src.gOff);
                                var b2 = *(pixel + src.bOff);

                                // TODO: Better color distance
                                var diff = (r + g + b) - (r2 + g2 + b2) + 256 * 3;
                                var weight = filter[filterX, filterY] * diffWeights[diff];
                                weightTotal += weight;
                                rTotal += r2 * weight;
                                gTotal += g2 * weight;
                                bTotal += b2 * weight;
                            }
                        }
                        r = rTotal / weightTotal;
                        g = gTotal / weightTotal;
                        b = bTotal / weightTotal;
                    }
                }
                *(dstPixel + dst.rOff) = (byte) r;
                *(dstPixel + dst.gOff) = (byte) g;
                *(dstPixel + dst.bOff) = (byte) b;
                if (copyAlpha)
                {
                    *(dstPixel + dst.aOff) = *(srcPixel + src.aOff);
                }
            }
        }
    }
}