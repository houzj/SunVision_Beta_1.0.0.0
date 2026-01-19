using System;
using VisionMaster.Interfaces;
using VisionMaster.Models;

namespace VisionMaster.Algorithms
{
    /// <summary>
    /// 边缘检测算法（Sobel算子）
    /// </summary>
    public class EdgeDetectionAlgorithm : BaseAlgorithm
    {
        public EdgeDetectionAlgorithm(ILogger logger) 
            : base("边缘检测", "使用Sobel算子检测图像边缘", logger)
        {
        }

        public override Mat Process(Mat image)
        {
            return Process(image, new AlgorithmParameters());
        }

        public override Mat Process(Mat image, AlgorithmParameters parameters)
        {
            // 如果是彩色图像，先转换为灰度
            if (image.Channels != 1)
            {
                var grayAlgo = new GrayScaleAlgorithm(Logger);
                image = grayAlgo.Process(image);
            }

            var threshold = parameters.HasParameter("Threshold") 
                ? parameters.GetParameter<int>("Threshold") 
                : 50;

            var result = new Mat(image.Width, image.Height, 1);

            // Sobel算子
            int[,] sobelX = {
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 }
            };

            int[,] sobelY = {
                { -1, -2, -1 },
                { 0, 0, 0 },
                { 1, 2, 1 }
            };

            for (int y = 1; y < image.Height - 1; y++)
            {
                for (int x = 1; x < image.Width - 1; x++)
                {
                    int gx = 0, gy = 0;

                    // 应用Sobel算子
                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            var pixel = image.Data[(y + ky) * image.Width + (x + kx)];
                            gx += pixel * sobelX[ky + 1, kx + 1];
                            gy += pixel * sobelY[ky + 1, kx + 1];
                        }
                    }

                    var magnitude = Math.Sqrt(gx * gx + gy * gy);
                    result.Data[y * result.Width + x] = magnitude > threshold ? (byte)255 : (byte)0;
                }
            }

            return result;
        }
    }
}
