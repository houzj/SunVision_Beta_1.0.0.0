using System;
using VisionMaster.Interfaces;
using VisionMaster.Models;

namespace VisionMaster.Algorithms
{
    /// <summary>
    /// 高斯模糊算法
    /// </summary>
    public class GaussianBlurAlgorithm : BaseAlgorithm
    {
        public GaussianBlurAlgorithm(ILogger logger) 
            : base("高斯模糊", "对图像进行高斯模糊处理", logger)
        {
        }

        public override Mat Process(Mat image)
        {
            // 默认核大小为3
            return Process(image, new AlgorithmParameters());
        }

        public override Mat Process(Mat image, AlgorithmParameters parameters)
        {
            var kernelSize = parameters.HasParameter("KernelSize") 
                ? parameters.GetParameter<int>("KernelSize") 
                : 3;

            // 确保核大小为奇数
            if (kernelSize % 2 == 0) kernelSize++;

            var result = new Mat(image.Width, image.Height, image.Channels);
            var halfKernel = kernelSize / 2;

            // 生成高斯核
            var kernel = GenerateGaussianKernel(kernelSize);

            // 应用高斯模糊
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    for (int c = 0; c < image.Channels; c++)
                    {
                        double sum = 0;
                        double weightSum = 0;

                        for (int ky = -halfKernel; ky <= halfKernel; ky++)
                        {
                            for (int kx = -halfKernel; kx <= halfKernel; kx++)
                            {
                                var nx = x + kx;
                                var ny = y + ky;

                                if (nx >= 0 && nx < image.Width && ny >= 0 && ny < image.Height)
                                {
                                    var pixel = image.Data[(ny * image.Width + nx) * image.Channels + c];
                                    var weight = kernel[ky + halfKernel, kx + halfKernel];
                                    
                                    sum += pixel * weight;
                                    weightSum += weight;
                                }
                            }
                        }

                        result.Data[(y * result.Width + x) * result.Channels + c] = (byte)(sum / weightSum);
                    }
                }
            }

            return result;
        }

        private double[,] GenerateGaussianKernel(int size)
        {
            var kernel = new double[size, size];
            var sigma = size / 3.0;
            var half = size / 2;
            var sum = 0.0;

            for (int y = -half; y <= half; y++)
            {
                for (int x = -half; x <= half; x++)
                {
                    var value = Math.Exp(-(x * x + y * y) / (2 * sigma * sigma));
                    kernel[y + half, x + half] = value;
                    sum += value;
                }
            }

            // 归一化
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    kernel[y, x] /= sum;
                }
            }

            return kernel;
        }
    }
}
