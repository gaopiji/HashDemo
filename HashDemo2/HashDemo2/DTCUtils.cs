using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashDemo2
{
    class DTCUtils
    {
        /// <summary>
        /// Discrete Cosine Transform
        /// </summary>
        /// <param name="pix">Original image pix matrix</param>
        /// <returns>Transformed matrix array</returns>
        public static int[,] DCT(int[,] pix)
    {
        int n = pix.GetLength(0);
        double[,] iMatrix = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                iMatrix[i, j] = pix[i, j];
            }
        }
        double[,] quotient = coefficient(n);
        double[,] quotientT = transposingMatrix(quotient);

        double[,] temp = new double[n, n];
        temp = matrixMultiply(quotient, iMatrix, n);
        iMatrix = matrixMultiply(temp, quotientT, n);

        int[,] newpix = new int[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                newpix[i, j] = (int)iMatrix[i, j];
            }
        }
        return newpix;
    }

    /// <summary>
    /// Matrix transpose
    /// </summary>
    /// <param name="matrix">Original matrix</param>
    /// <returns>Transposed matrix</returns>
    private static double[,] transposingMatrix(double[,] matrix)
    {
        int row = matrix.GetLength(0);
        int col = matrix.GetLength(1);
        double[,] nMatrix = new double[col, row];
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                nMatrix[j, i] = matrix[i, j];
            }
        }
        return nMatrix;
    }

    /// <summary>
    /// Find the coefficient matrix of discrete cosine transform
    /// </summary>
    /// <param name="n">The size of n*n matrix</param>
    /// <returns>Coefficient matrix</returns>
    private static double[,] coefficient(int n)
    {
        double[,] coeff = new double[n, n];
        double sqrt = 1.0 / Math.Sqrt(n);
        for (int i = 0; i < n; i++)
        {
            coeff[0, i] = sqrt;
        }
        for (int i = 1; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                coeff[i, j] = Math.Sqrt(2.0 / n) * Math.Cos(i * Math.PI * (j + 0.5) / (double)n);
            }
        }
        return coeff;
    }

    /// <summary>
    /// Matrix Multiplication
    /// </summary>
    /// <param name="A">Matrix A</param>
    /// <param name="B">Matrix B</param>
    /// <param name="n">The size of n*n matrix</param>
    /// <returns>Coefficient matrix</returns>
    private static double[,] matrixMultiply(double[,] A, double[,] B, int n)
    {
        double[,] nMatrix = new double[n, n];
        double t = 0.0;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                t = 0;
                for (int k = 0; k < n; k++)
                {
                    t += A[i, k] * B[k, j];
                }
                nMatrix[i, j] = t;
            }
        }
        return nMatrix;
    }

        private const double SQRT_TWO = 1.4142135623730950488016887242097;
        /// <summary>
        /// Compute the dct of a given vector
        /// </summary>
        /// <param name="featureVector">vector of input series</param>
        /// <returns>the dct of R</returns>
        public static Digest ComputeDct(double[] featureVector)
        {
            var N = featureVector.Length;

            var digest = new Digest();

            var R = featureVector;
            var D = digest.Coefficents;

            var D_temp = new double[Digest.LENGTH];
            double max = 0.0;
            double min = 0.0;
            for (int k = 0; k < Digest.LENGTH; k++)
            {
                double sum = 0.0;
                for (int n = 0; n < N; n++)
                {
                    double temp = R[n] * Math.Cos((Math.PI * (2 * n + 1) * k) / (2 * N));
                    sum += temp;
                }
                if (k == 0)
                {
                    D_temp[k] = sum / Math.Sqrt(N);
                }
                else
                {
                    D_temp[k] = sum * SQRT_TWO / Math.Sqrt((double)N);
                }
                if (D_temp[k] > max)
                {
                    max = D_temp[k];
                }
                if (D_temp[k] < min)
                {
                    min = D_temp[k];
                }
            }

            for (int i = 0; i < Digest.LENGTH; i++)
            {
                D[i] = (byte)(byte.MaxValue * (D_temp[i] - min) / (max - min));
            }

            return digest;
        }
    }
   
}
