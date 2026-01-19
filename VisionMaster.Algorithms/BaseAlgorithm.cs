using System;
using VisionMaster.Interfaces;
using VisionMaster.Models;

namespace VisionMaster.Algorithms
{
    /// <summary>
    /// 算法基类
    /// </summary>
    public abstract class BaseAlgorithm : IImageProcessor
    {
        /// <summary>
        /// 算法名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 算法描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        protected ILogger Logger { get; set; }

        protected BaseAlgorithm(string name, string description, ILogger logger)
        {
            Name = name;
            Description = description;
            Logger = logger;
        }

        /// <summary>
        /// 处理图像
        /// </summary>
        public abstract Mat Process(Mat image);

        /// <summary>
        /// 处理图像（带参数）
        /// </summary>
        public virtual Mat Process(Mat image, AlgorithmParameters parameters)
        {
            return Process(image);
        }

        /// <summary>
        /// 执行算法
        /// </summary>
        public AlgorithmResult Execute(Mat image, AlgorithmParameters parameters = null)
        {
            var startTime = DateTime.Now;

            try
            {
                var resultImage = parameters != null
                    ? Process(image, parameters)
                    : Process(image);

                var executionTime = (long)(DateTime.Now - startTime).TotalMilliseconds;

                Logger.LogInfo($"算法 {Name} 执行成功，耗时: {executionTime}ms");

                return AlgorithmResult.CreateSuccess(resultImage, executionTime);
            }
            catch (Exception ex)
            {
                var errorMessage = $"算法 {Name} 执行失败: {ex.Message}";
                Logger.LogError(errorMessage, ex);

                return AlgorithmResult.CreateError(errorMessage);
            }
        }

        /// <summary>
        /// IImageProcessor接口实现
        /// </summary>
        public object? Process(object image)
        {
            if (image is Mat mat)
            {
                return Process(mat);
            }
            throw new ArgumentException($"不支持的图像类型: {image?.GetType()}");
        }
    }
}
