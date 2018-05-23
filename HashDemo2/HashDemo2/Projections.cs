using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashDemo2
{
    /// <summary>
    /// Radon Projection info
    /// </summary>
    public class Projections
    {
        private int Width;
        private int Height;
        public Projections(int regionHeight, int regionWidth, int lineCount)
        {
            Region = new float[regionHeight, regionWidth];
            PixelsPerLine = new int[lineCount];
            Width = regionWidth;
            Height = regionHeight;
        }

        /// <summary>
        /// contains projections of image of angled lines through center
        /// </summary>
        public float[,] Region { get; }

        /// <summary>
        /// int array denoting the number of pixels of each line
        /// </summary>
        public int[] PixelsPerLine { get; }

        private static float ROUNDING_FACTOR(float x)
        => x >= 0 ? 0.5f : -0.5f;
        private static double ROUNDING_FACTOR(double x)
        => x >= 0 ? 0.5 : -0.5;
        /// <summary>
        /// Find radon projections of N lines running through the image center for lines angled 0 to 180 degrees from horizontal.
        /// </summary>
        /// <param name="img">CImg src image</param>
        /// <param name="numberOfLines">int number of angled lines to consider.</param>
        /// <returns>Projections struct</returns>
        public static Projections FindRadonProjections(float[,] img, int numberOfLines)
        {
            var width = img.GetLength(1);
            var height = img.GetLength(0);
            int D = (width > height) ? width : height;
            var x_center = width / 2f;
            var y_center = height / 2f;
            var x_off = (int)Math.Floor(x_center + ROUNDING_FACTOR(x_center));
            var y_off = (int)Math.Floor(y_center + ROUNDING_FACTOR(y_center));

            var projs = new Projections(numberOfLines, D, numberOfLines);

            var radonMap = projs.Region;
            var ppl = projs.PixelsPerLine;

            for (var k = 0; k < numberOfLines / 4 + 1; k++)
            {
                var theta = k * Math.PI / numberOfLines;
                var alpha = Math.Tan(theta);
                for (var x = 0; x < D; x++)
                {
                    var y = alpha * (x - x_off);
                    var yd = (int)Math.Floor(y + ROUNDING_FACTOR(y));
                    if ((yd + y_off >= 0) && (yd + y_off < height) && (x < width))
                    {
                        radonMap[k, x] = img[x, yd + y_off];
                        ppl[k] += 1;
                    }
                    if ((yd + x_off >= 0) && (yd + x_off < width) && (k != numberOfLines / 4) && (x < height))
                    {
                        radonMap[numberOfLines / 2 - k, x] = img[yd + x_off, x];
                        ppl[numberOfLines / 2 - k] += 1;
                    }
                }
            }
            var j = 0;
            for (var k = 3 * numberOfLines / 4; k < numberOfLines; k++)
            {
                var theta = k * Math.PI / numberOfLines;
                var alpha = Math.Tan(theta);
                for (var x = 0; x < D; x++)
                {
                    var y = alpha * (x - x_off);
                    var yd = (int)Math.Floor(y + ROUNDING_FACTOR(y));
                    if ((yd + y_off >= 0) && (yd + y_off < height) && (x < width))
                    {
                        radonMap[k, x] = img[x, yd + y_off];
                        ppl[k] += 1;
                    }
                    if ((y_off - yd >= 0) && (y_off - yd < width) && (2 * y_off - x >= 0) && (2 * y_off - x < height) && (k != 3 * numberOfLines / 4))
                    {
                        radonMap[k - j, x] = img[-yd + y_off, -(x - y_off) + y_off];
                        ppl[k - j] += 1;
                    }
                }
                j += 2;
            }

            return projs;
        }

        /// <summary>
        /// compute the feature vector from a radon projection map.
        /// </summary>
        /// <param name="projections">Projections struct</param>
        /// <returns>Features struct</returns>
        public static double[] ComputeFeatureVector(Projections projections)
        {
            var map = projections.Region;
            var ppl = projections.PixelsPerLine;
            var N = ppl.Length;
            var D = projections.Width;

            var feat_v = new double[N];
            var sum = 0.0;
            var sum_sqd = 0.0;
            for (int k = 0; k < N; k++)
            {
                var line_sum = 0.0;
                var line_sum_sqd = 0.0;
                var nb_pixels = ppl[k];
                for (var i = 0; i < D; i++)
                {
                    line_sum += map[k, i];
                    line_sum_sqd += map[k, i] * map[k, i];
                }
                feat_v[k] = nb_pixels > 0 ? (line_sum_sqd / nb_pixels) - (line_sum * line_sum) / (nb_pixels * nb_pixels) : 0;
                sum += feat_v[k];
                sum_sqd += feat_v[k] * feat_v[k];
            }
            var mean = sum / N;
            var var = Math.Sqrt((sum_sqd / N) - (sum * sum) / (N * N));

            for (var i = 0; i < N; i++)
            {
                feat_v[i] = (feat_v[i] - mean) / var; 
            }

            return feat_v;
        }
    }
}
