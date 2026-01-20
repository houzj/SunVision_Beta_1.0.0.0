using System;
using System.Threading;
using VisionMaster.Interfaces;
using VisionMaster.Models;

namespace VisionMaster.DeviceDriver
{
    /// <summary>
    /// 模拟相机驱动（用于测试）
    /// </summary>
    public class SimulatedCameraDriver : BaseDeviceDriver
    {
        private bool _isCapturing;
        private Thread _captureThread;
        private Random _random;

        public SimulatedCameraDriver(string deviceId, ILogger logger)
            : base(deviceId, "模拟相机", "SimulatedCamera", logger)
        {
            _random = new Random();
        }

        public override bool Connect()
        {
            try
            {
                Logger.LogInfo($"正在连接设备: {DeviceName} (ID: {DeviceId})");
                
                // 模拟连接延迟
                Thread.Sleep(100);

                IsConnected = true;
                Logger.LogInfo($"设备连接成功: {DeviceName}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"设备连接失败: {DeviceName}", ex);
                return false;
            }
        }

        public override bool Disconnect()
        {
            try
            {
                if (_isCapturing)
                {
                    StopContinuousCapture();
                }

                Logger.LogInfo($"正在断开设备: {DeviceName}");
                IsConnected = false;
                Logger.LogInfo($"设备断开成功: {DeviceName}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"设备断开失败: {DeviceName}", ex);
                return false;
            }
        }

        public override Mat CaptureImage()
        {
            if (!IsConnected)
            {
                Logger.LogError("设备未连接，无法获取图像");
                return null;
            }

            try
            {
                Logger.LogDebug($"从设备 {DeviceName} 获取图像");
                
                // 生成模拟图像数据
                var width = 640;
                var height = 480;
                var channels = 3;
                var data = new byte[width * height * channels];

                for (int i = 0; i < data.Length; i += 3)
                {
                    // 生成随机彩色图像
                    data[i] = (byte)_random.Next(50, 200);     // R
                    data[i + 1] = (byte)_random.Next(50, 200); // G
                    data[i + 2] = (byte)_random.Next(50, 200); // B
                }

                Logger.LogDebug($"图像获取成功: {width}x{height}");
                return new Mat(data, width, height, channels);
            }
            catch (Exception ex)
            {
                Logger.LogError($"获取图像失败: {DeviceName}", ex);
                return null;
            }
        }

        public override bool StartContinuousCapture()
        {
            if (!IsConnected)
            {
                Logger.LogError("设备未连接，无法开始连续采集");
                return false;
            }

            if (_isCapturing)
            {
                Logger.LogWarning("已经在连续采集模式中");
                return true;
            }

            try
            {
                _isCapturing = true;
                _captureThread = new Thread(CaptureLoop)
                {
                    IsBackground = true
                };
                _captureThread.Start();
                
                Logger.LogInfo($"开始连续采集: {DeviceName}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"开始连续采集失败: {DeviceName}", ex);
                return false;
            }
        }

        public override bool StopContinuousCapture()
        {
            if (!_isCapturing)
            {
                return true;
            }

            try
            {
                _isCapturing = false;
                _captureThread?.Join(1000);
                Logger.LogInfo($"停止连续采集: {DeviceName}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"停止连续采集失败: {DeviceName}", ex);
                return false;
            }
        }

        private void CaptureLoop()
        {
            while (_isCapturing)
            {
                try
                {
                    var image = CaptureImage();
                    // 在实际应用中，这里会触发图像获取事件
                    Thread.Sleep(100); // 模拟帧率
                }
                catch
                {
                    // 忽略异常，继续采集
                }
            }
        }

        public override DeviceInfo GetDeviceInfo()
        {
            return new DeviceInfo
            {
                DeviceId = DeviceId,
                DeviceName = DeviceName,
                DeviceType = DeviceType,
                IsConnected = IsConnected,
                Description = "模拟相机设备，用于测试"
            };
        }

        public override bool SetParameter(string key, object value)
        {
            Logger.LogInfo($"设置设备参数: {key} = {value}");
            return true;
        }

        public override T GetParameter<T>(string key)
        {
            Logger.LogInfo($"获取设备参数: {key}");
            return default(T);
        }
    }
}
