using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VisionMaster.Interfaces;
using VisionMaster.Models;

namespace VisionMaster.DeviceDriver
{
    /// <summary>
    /// 设备管理器
    /// </summary>
    public class DeviceManager : IDeviceManager
    {
        /// <summary>
        /// 已注册的设备驱动列表
        /// </summary>
        private Dictionary<string, BaseDeviceDriver> _registeredDrivers;

        /// <summary>
        /// 日志记录器
        /// </summary>
        private ILogger Logger { get; set; }

        public DeviceManager(ILogger logger)
        {
            Logger = logger;
            _registeredDrivers = new Dictionary<string, BaseDeviceDriver>();
        }

        /// <summary>
        /// 注册设备驱动
        /// </summary>
        public void RegisterDriver(BaseDeviceDriver driver)
        {
            if (_registeredDrivers.ContainsKey(driver.DeviceId))
            {
                Logger.LogWarning($"设备驱动已存在: {driver.DeviceId}");
                return;
            }

            _registeredDrivers[driver.DeviceId] = driver;
            Logger.LogInfo($"注册设备驱动: {driver.DeviceName} (ID: {driver.DeviceId})");
        }

        /// <summary>
        /// 注销设备驱动
        /// </summary>
        public bool UnregisterDriver(string deviceId)
        {
            if (_registeredDrivers.TryGetValue(deviceId, out var driver))
            {
                if (driver.IsConnected)
                {
                    driver.Disconnect();
                }

                driver.Dispose();
                _registeredDrivers.Remove(deviceId);
                Logger.LogInfo($"注销设备驱动: {driver.DeviceName} (ID: {deviceId})");
                return true;
            }

            return false;
        }

        public async Task<List<DeviceInfo>> DetectDevicesAsync()
        {
            var devices = new List<DeviceInfo>();

            Logger.LogInfo("开始检测可用设备");

            foreach (var driver in _registeredDrivers.Values)
            {
                try
                {
                    var info = driver.GetDeviceInfo();
                    devices.Add(info);
                    Logger.LogInfo($"检测到设备: {info.DeviceName}");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"检测设备失败: {driver.DeviceName}", ex);
                }
            }

            Logger.LogInfo($"检测完成，共发现 {devices.Count} 个设备");

            await Task.CompletedTask;
            return devices;
        }

        public async Task<bool> ConnectDeviceAsync(string deviceId)
        {
            if (!_registeredDrivers.TryGetValue(deviceId, out var driver))
            {
                Logger.LogError($"设备驱动不存在: {deviceId}");
                await Task.CompletedTask;
                return false;
            }

            try
            {
                var result = driver.Connect();
                await Task.CompletedTask;
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError($"连接设备失败: {deviceId}", ex);
                await Task.CompletedTask;
                return false;
            }
        }

        public async Task<bool> DisconnectDeviceAsync(string deviceId)
        {
            if (!_registeredDrivers.TryGetValue(deviceId, out var driver))
            {
                Logger.LogError($"设备驱动不存在: {deviceId}");
                await Task.CompletedTask;
                return false;
            }

            try
            {
                var result = driver.Disconnect();
                await Task.CompletedTask;
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError($"断开设备失败: {deviceId}", ex);
                await Task.CompletedTask;
                return false;
            }
        }

        public async Task<Mat> CaptureImageAsync(string deviceId)
        {
            if (!_registeredDrivers.TryGetValue(deviceId, out var driver))
            {
                Logger.LogError($"设备驱动不存在: {deviceId}");
                await Task.CompletedTask;
                return null;
            }

            if (!driver.IsConnected)
            {
                Logger.LogError($"设备未连接: {deviceId}");
                await Task.CompletedTask;
                return null;
            }

            try
            {
                var image = driver.CaptureImage();
                await Task.CompletedTask;
                return image;
            }
            catch (Exception ex)
            {
                Logger.LogError($"获取图像失败: {deviceId}", ex);
                await Task.CompletedTask;
                return null;
            }
        }

        public List<string> GetConnectedDevices()
        {
            return _registeredDrivers.Values
                .Where(d => d.IsConnected)
                .Select(d => d.DeviceId)
                .ToList();
        }

        /// <summary>
        /// 获取已注册的设备列表
        /// </summary>
        public List<DeviceInfo> GetRegisteredDevices()
        {
            return _registeredDrivers.Values
                .Select(d => d.GetDeviceInfo())
                .ToList();
        }

        /// <summary>
        /// 释放所有资源
        /// </summary>
        public void Dispose()
        {
            var deviceIds = _registeredDrivers.Keys.ToList();
            foreach (var deviceId in deviceIds)
            {
                UnregisterDriver(deviceId);
            }
        }
    }
}
