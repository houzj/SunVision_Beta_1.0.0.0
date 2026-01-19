using System;
using VisionMaster.Interfaces;
using VisionMaster.Models;

namespace VisionMaster.Algorithms
{
    /// <summary>
    /// 二值化算法
    /// </summary>
    public class ThresholdAlgorithm : BaseAlgorithm
    {
        public ThresholdAlgorithm(ILogger logger) 
            : base("二值化", "将灰度图像转换为二值图像", logger)
        {
        }

        public override Mat Process(Mat image)
        {
            // 默认阈值128
            return Process(image, new AlgorithmParameters());
        }

        public override Mat Process(Mat image, AlgorithmParameters parameters)
        {
            if (image.Channels != 1)
            {
                // 如果不是灰度图像，先转换为灰度
                var grayAlgo = new GrayScaleAlgorithm(Logger);
                image = grayAlgo.Process(image);
            }

            var threshold = parameters.HasParameter("Threshold") 
                ? parameters.GetParameter<int>("Threshold") 
                : 128;

            var binaryData = new byte[image.Width * image.Height];
            
            for (int i = 0; i < image.Data.Length; i++)
            {
                binaryData[i] = image.Data[i] >= threshold ? (byte)255 : (byte)0;
            }

            return new Mat(binaryData, image.Width, image.Height, 1);
        }
    }
}
