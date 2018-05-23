using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashDemo2
{
    class Program
    {

        public const int N_DCT = 8;

        private static string inputFolder = "";
        private static string outputFolder = "";
        private static int folderCount = 1;

        private static float similarity = 0.75f;
        private static int distance = 15;

        public static bool ShowUsage = false;

        private static void Usage()
        {
            Console.WriteLine("Find Similar Images.\n");

            Console.WriteLine("HashhDemo2 inputfolder -out:folder [-similarity:n]  [-distance:n]");

            Console.WriteLine("  inputfolder ");
            Console.WriteLine("                 Input source folders.");
            Console.WriteLine("  -out:folder    output root folder.");
            Console.WriteLine("  -distance:n    Image Hamming distance threshold number n ranging 0 to 64.Defaults is 15");
            Console.WriteLine("  -similarity:n  Image similarity threshold number n ranging 0.0 to 1.0.Defaults is 0.75");
            Console.WriteLine();
            Console.WriteLine("Remarks:");
            Console.WriteLine("only JPG/JPEG, BMP, PNG image are supported.");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine(@"  HashhDemo2  c:\folder1 -out:c:\output");
            Console.WriteLine(@"  HashhDemo2  c:\folder1 -out:c:\output -similarity:0.7");
            Console.WriteLine(@"  HashhDemo2  c:\folder1 -out:c:\output -distance:20");
            Console.WriteLine(@"  HashhDemo2  c:\folder1 -out:c:\output -distance:20 -similarity:0.7");

            Console.WriteLine();
        }

        static void Main(string[] args)
        {

            for (var i = 0; i < args.Length; ++i)
            {
                var arg = args[i].ToLower().Trim();
                if (!arg.StartsWith("/") && !arg.StartsWith("-"))
                {
                    inputFolder = Path.GetFullPath(args[0].Trim());
                }
                else if (arg.StartsWith("-out:"))
                {
                    outputFolder = arg.Substring(arg.IndexOf(':') + 1).Trim();
                }

                else if (StringMatch(arg, "/help", "-help", "/h", "-h", "/?", "-?"))
                {
                    ShowUsage = true;
                    break;
                }

                else if (arg.StartsWith("-similarity:"))
                {

                    similarity = float.Parse(arg.Substring(arg.IndexOf(':') + 1).Trim());
                      if (similarity < 0.0 || similarity > 1.0)
                       {
                            Console.WriteLine("similarity should be within range 0.0 and 1.0.");
                        similarity = 0.75f;
                       }
                }

                else if (arg.StartsWith("-distance:"))
                {

                    distance = int.Parse(arg.Substring(arg.IndexOf(':') + 1).Trim());
                    if (distance < 0 || distance > 64)
                    {
                        Console.WriteLine("distance threshold should be within range 0 and 64.");
                        distance = 15;
                    }
                }
            }

            if (null == args || 0 == args.Length)
            {
                ShowUsage = true;
            }
            if (ShowUsage)
            {
                Usage();
                return ;
            }

            // CalculateHammingDistance(inputFolder);
            //CalculateCrossCorrelation(inputFolder);

            //Console.WriteLine($"similarity : {similarity}, distance : {distance}");
            CalculateTwoFunction(inputFolder);
        }

        private static bool StringMatch(string str, params string[] values)
        {
            return values.Any(str.Equals);
        }

        /// <summary>
        /// Find similar images under folder
        /// </summary>
        /// <param name="folderPath">folderPath</param>
        private static void CalculateHammingDistance(string folderPath)
        {

            List<string> imagesPath = Directory.GetFiles(folderPath, "*.jpg").ToList();
            imagesPath.AddRange(Directory.GetFiles(folderPath, "*.bmp").ToList());
            imagesPath.AddRange(Directory.GetFiles(folderPath, "*.png").ToList());
            Dictionary<string, string> nameMap = new Dictionary<string, string>();
            List<KeyValuePair<string, bool[,]>> imageHashList = new List<KeyValuePair<string, bool[,]>>();
            for (int i = 0; i < imagesPath.Count(); i++)
            {
                string imageName = Path.GetFileName(imagesPath[i]);
                nameMap.Add(imageName, imagesPath[i]);
                bool[,] hash = CalculateHashMatrix(imagesPath[i]);
                KeyValuePair<string, bool[,]> pair = new KeyValuePair<string, bool[,]>(imageName, hash);
                imageHashList.Add(pair);
            }

            for (int i = 0; i < imageHashList.Count(); i++)
            {
                string image1 = imageHashList[i].Key;
                bool[,] hash1 = imageHashList[i].Value;
                for (int j = i + 1; j < imageHashList.Count(); j++)
                {
                    string image2 = imageHashList[j].Key;
                    bool[,] hash2 = imageHashList[j].Value;
                    int dis = CalculateHammingDistance(hash1, hash2);
                    if (dis < 15)
                    {
                        Console.WriteLine($"image1:{image1},image2:{image2}-->dis:{dis}");
                        Console.WriteLine($"");
                        folderCount++;
                        if (dis < 15)
                        {
                            string copyPath = CreateDir(Path.Combine(outputFolder, folderCount + ""));
                            
                            File.Copy(nameMap[image1], Path.Combine(copyPath, image1));
                            File.Copy(nameMap[image2], Path.Combine(copyPath, image2));
                        }

                    }

                }
            }
        }

        /// <summary>
        /// Find similar images under folder
        /// </summary>
        /// <param name="folderPath">folderPath</param>
        private static void CalculateCrossCorrelation(string folderPath)
        {

            List<string> imagesPath = Directory.GetFiles(folderPath, "*.jpg").ToList();
            imagesPath.AddRange(Directory.GetFiles(folderPath, "*.bmp").ToList());
            imagesPath.AddRange(Directory.GetFiles(folderPath, "*.png").ToList());
            Dictionary<string, string> nameMap = new Dictionary<string, string>();
            List<KeyValuePair<string,Digest>> imageHashList = new List<KeyValuePair<string, Digest>>();
            for (int i = 0; i < imagesPath.Count(); i++)
            {
                string imageName = Path.GetFileName(imagesPath[i]);
                nameMap.Add(imageName, imagesPath[i]);
                Digest hash = CalculateCoefficients(imagesPath[i]);
                KeyValuePair<string, Digest> pair = new KeyValuePair<string, Digest>(imageName, hash);
                imageHashList.Add(pair);
            }

            for (int i = 0; i < imageHashList.Count(); i++)
            {
                string image1 = imageHashList[i].Key;
                Digest hash1 = imageHashList[i].Value;
                for (int j = i + 1; j < imageHashList.Count(); j++)
                {
                    string image2 = imageHashList[j].Key;
                    Digest hash2 = imageHashList[j].Value;
                    double dis = GetCrossCorrelationCore(hash1.Coefficents, hash2.Coefficents);
                    if (dis >0.9)
                    {
                        Console.WriteLine($"image1:{image1},image2:{image2}-->dis:{dis}");
                       // Console.WriteLine($"");
                        if (dis >0.9)
                        {
                            string copyPath = CreateDir(Path.Combine(outputFolder, folderCount + ""));
                            folderCount++;
                            File.Copy(nameMap[image1], Path.Combine(copyPath, image1));
                            File.Copy(nameMap[image2], Path.Combine(copyPath, image2));
                        }

                    }

                }
            }
        }


        struct TwoFeature
        {
            public Digest digest;
            public bool[,] hash;
        }
        /// <summary>
        /// Find similar images under folder
        /// </summary>
        /// <param name="folderPath">folderPath</param>
        private static void CalculateTwoFunction(string folderPath)
        {

            List<string> imagesPath = Directory.GetFiles(folderPath, "*.jpg").ToList();
            imagesPath.AddRange(Directory.GetFiles(folderPath, "*.bmp").ToList());
            imagesPath.AddRange(Directory.GetFiles(folderPath, "*.png").ToList());
            Dictionary<string, string> nameMap = new Dictionary<string, string>();
            List<KeyValuePair<string, TwoFeature>> imageHashList = new List<KeyValuePair<string, TwoFeature>>();
            for (int i = 0; i < imagesPath.Count(); i++)
            {
                string imageName = Path.GetFileName(imagesPath[i]);
                nameMap.Add(imageName, imagesPath[i]);

                TwoFeature feature = new TwoFeature();
                Digest digest = CalculateCoefficients(imagesPath[i]);
                bool[,] phash = CalculateHashMatrix(imagesPath[i]);
                feature.digest = digest; feature.hash = phash;
                KeyValuePair<string, TwoFeature> pair = new KeyValuePair<string, TwoFeature>(imageName, feature);
                imageHashList.Add(pair);
            }

            for (int i = 0; i < imageHashList.Count(); i++)
            {
                string image1 = imageHashList[i].Key;
                TwoFeature feature1 = imageHashList[i].Value;
                for (int j = i + 1; j < imageHashList.Count(); j++)
                {
                    string image2 = imageHashList[j].Key;
                    TwoFeature feature2 = imageHashList[j].Value;
                    double disSimilarity = GetCrossCorrelationCore(feature1.digest.Coefficents, feature2.digest.Coefficents);
                    int dis = CalculateHammingDistance(feature1.hash, feature2.hash);
                    if (disSimilarity > similarity-0.1 && dis< distance+10)
                    {
                        Console.WriteLine($"index:{folderCount}: image1:{image1},image2:{image2}-->dis:{dis} , disSimilarity:{disSimilarity}");
                        Console.WriteLine($"");
                       
                        if (disSimilarity > similarity && dis < distance)
                        {
                            string copyPath = CreateDir(Path.Combine(outputFolder, folderCount + ""));
                          
                            File.Copy(nameMap[image1], Path.Combine(copyPath, image1));
                            File.Copy(nameMap[image2], Path.Combine(copyPath, image2));
                        }
                        folderCount++;

                    }

                }
            }
        }

        public static string CreateDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        /// <summary>
        /// Calculate two hash's hamming distance
        /// </summary>
        /// <param name="hash1">image1 hash mtrix</param>
        /// <param name="hash2">image2 hash mtrix</param>
        private static int CalculateHammingDistance(bool[,] hash1, bool[,] hash2)
        {
            int distance = 0;
            for (int i = 0; i < hash1.GetLength(0); i++)
            {
                for (int j = 0; j < hash1.GetLength(1); j++)
                {
                    if (hash1[i, j] != hash2[i, j])
                    {
                        distance++;
                    }
                }
            }

            distance -= hash1[0, 0] != hash2[0, 0] ? 1 : 0;
            return distance;
        }

        /// <summary>
        /// Calculating the Similarity of Two feature vector with NCC
        /// </summary>
        /// <param name="coefficients1">coefficients1</param>
        /// <param name="coefficients2">coefficients2</param>
        /// <returns>similarity</returns>
        static double GetCrossCorrelationCore(byte[] coefficients1, byte[] coefficients2)
        {
            int length = Math.Min(coefficients1.Length, coefficients2.Length);
            var sumx = 0.0;
            var sumy = 0.0;
            for (var i = 0; i < length; i++)
            {
                sumx += coefficients1[i];
                sumy += coefficients2[i];
            }

            var meanx = sumx / length;
            var meany = sumy / length;
            var max = 0.0;
            for (var d = 0; d < length; d++)
            {
                var num = 0.0;
                var denx = 0.0;
                var deny = 0.0;

                for (var i = 0; i < length; i++)
                {
                    var dx = coefficients1[i] - meanx;
                    var dy = coefficients2[(length + i - d) % length] - meany;
                    num += dx * dy;
                    denx += dx * dx;
                    deny += dy * dy;
                }
                var r = num < 0 || denx == 0 || deny == 0 ? double.NaN : (num * num / (denx * deny));
                if (r > max)
                {
                    max = r;
                }
            }

            return Math.Sqrt(max);
        }

        protected const int DEFAULT_NUMBER_OF_ANGLES = 180;
        /// <summary>
        /// Calculate the Coefficients
        /// </summary>
        /// <param name="imagePath">imagePath to be calculated</param>
        private static Digest CalculateCoefficients(string imagePath)
        {
            Image image = Image.FromFile(imagePath);

            Bitmap bitmap = ResetImageSize(image, N);

            bitmap = ToGray(bitmap);

            float[,] bitPix = new float[bitmap.Height, bitmap.Width];
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    Color c = bitmap.GetPixel(i, j);
                    bitPix[j, i] = c.G;
                }
            }

            var projs = Projections.FindRadonProjections(bitPix, DEFAULT_NUMBER_OF_ANGLES);
            var features = Projections.ComputeFeatureVector(projs);

            return DTCUtils.ComputeDct(features);
        }

        /// <summary>
        /// Calculate the hash matrix
        /// </summary>
        /// <param name="imagePath">imagePath to be calculated</param>
        private static bool[,] CalculateHashMatrix(string imagePath)
        {
            Image image = Image.FromFile(imagePath);

            //Console.WriteLine($"image:{Path.GetFileName(imagePath)}, Width:{image.Width}, Height:{image.Height}");

            Bitmap bitmap = ResetImageSize(image, N);

            bitmap = ToGray(bitmap);

            int[,] dtc = calculateDCT(bitmap);
            int average = CalculateDTCAverage(dtc);

            bool[,] hashMatrix = CompareDtc(average, dtc);

            return hashMatrix;
        }

        public const int N = 32;
        // 1 Reduce size
        /// <summary>
        /// Reduce the size of the picture to NxN.
        /// </summary>
        /// <param name="Image">Image to be reduced</param>
        public static Bitmap ResetImageSize(Image image, int N)
        {
            if (image == null) return null;
            Image img = null;

            int ImageWidth = N;
            int ImageHeight = N;
            img = image.GetThumbnailImage(ImageWidth, ImageHeight, null, IntPtr.Zero);
            return new Bitmap(img);
        }

        // 2 Simplify color
        /// <summary>
        /// Convert pictures to grayscale images
        /// </summary>
        /// <param name="image">Image to be grayscale</param>
        private static Bitmap ToGray(Bitmap image)
        {
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color c = image.GetPixel(i, j);
                    int rgb = (c.R * 19595 + c.G * 38469 + c.B * 7472) >> 16;
                    image.SetPixel(i, j, Color.FromArgb(rgb, rgb, rgb));
                }
            }
            return image;
        }

        // 3 Calculate DCT
        /// <summary>
        /// Calculate images DTC
        /// </summary>
        /// <param name="image">Image to be grayscale</param>
        private static int[,] calculateDCT(Bitmap image)
        {
            int[,] pix = new int[N, N];
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color c = image.GetPixel(i, j);
                    pix[i, j] = c.R;
                }
            }
            int[,] dtcTemp = DTCUtils.DCT(pix);
            int[,] reDtc = new int[N_DCT, N_DCT];
            for (int i = 0; i < N_DCT; i++)
            {
                for (int j = 0; j < N_DCT; j++)
                {

                    reDtc[i, j] = dtcTemp[i, j];
                }
            }
            return reDtc;
        }

        // 4 Calculate the average
        /// <summary>
        /// Calculate the gray average of all 64 pixels.
        /// </summary>
        /// <param name="image">Image to be calculated</param>
        private static int CalculateDTCAverage(int[,] dtc)
        {
            int sum = 0;
            for (int i = 0; i < N_DCT; i++)
            {
                for (int j = 0; j < N_DCT; j++)
                {
                    sum += dtc[i, j];
                }
            }
            sum -= dtc[0, 0];
            return sum / (N_DCT * N_DCT - 1);
        }

        // 5 Compare pixel grayscales
        /// <summary>
        /// Compare the gray level of each pixel with the average.
        /// </summary>
        /// <param name="average">gray average</param>
        /// <param name="image">Image to be Compared</param>
        private static bool[,] CompareDtc(int average, int[,] dtc)
        {
            bool[,] comps = new bool[N_DCT, N_DCT];
            for (int i = 0; i < N_DCT; i++)
            {
                for (int j = 0; j < N_DCT; j++)
                {
                    comps[j, i] = dtc[i, j] > average;
                }
            }
            return comps;
        }
    }
}
