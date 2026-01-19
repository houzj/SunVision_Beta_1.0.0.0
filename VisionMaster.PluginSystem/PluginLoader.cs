using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using VisionMaster.Interfaces;

namespace VisionMaster.PluginSystem
{
    /// <summary>
    /// 插件加载器
    /// </summary>
    public class PluginLoader
    {
        private readonly ILogger _logger;
        private readonly Dictionary<string, IVisionPlugin> _loadedPlugins;

        public PluginLoader(ILogger logger)
        {
            _logger = logger;
            _loadedPlugins = new Dictionary<string, IVisionPlugin>();
        }

        /// <summary>
        /// 从目录加载所有插件
        /// </summary>
        /// <param name="pluginDirectory">插件目录</param>
        public void LoadPluginsFromDirectory(string pluginDirectory)
        {
            if (!Directory.Exists(pluginDirectory))
            {
                _logger.LogWarning($"插件目录不存在: {pluginDirectory}");
                return;
            }

            var dllFiles = Directory.GetFiles(pluginDirectory, "*.dll", SearchOption.TopDirectoryOnly);
            
            foreach (var dllFile in dllFiles)
            {
                try
                {
                    LoadPlugin(dllFile);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"加载插件失败: {dllFile}", ex);
                }
            }

            _logger.LogInfo($"已加载 {_loadedPlugins.Count} 个插件");
        }

        /// <summary>
        /// 加载单个插件
        /// </summary>
        /// <param name="pluginPath">插件DLL路径</param>
        public void LoadPlugin(string pluginPath)
        {
            _logger.LogInfo($"正在加载插件: {pluginPath}");

            var assembly = Assembly.LoadFrom(pluginPath);
            var pluginTypes = assembly.GetTypes()
                .Where(t => typeof(IVisionPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var pluginType in pluginTypes)
            {
                try
                {
                    var plugin = (IVisionPlugin)Activator.CreateInstance(pluginType);
                    
                    // 检查插件ID是否已存在
                    if (_loadedPlugins.ContainsKey(plugin.PluginId))
                    {
                        _logger.LogWarning($"插件ID {plugin.PluginId} 已存在，跳过加载");
                        continue;
                    }

                    // 初始化插件
                    plugin.Initialize();
                    _loadedPlugins[plugin.PluginId] = plugin;
                    
                    _logger.LogInfo($"插件加载成功: {plugin.Name} v{plugin.Version}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"实例化插件失败: {pluginType.Name}", ex);
                }
            }
        }

        /// <summary>
        /// 卸载插件
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        public bool UnloadPlugin(string pluginId)
        {
            if (!_loadedPlugins.TryGetValue(pluginId, out var plugin))
            {
                _logger.LogWarning($"插件未找到: {pluginId}");
                return false;
            }

            try
            {
                plugin.Unload();
                _loadedPlugins.Remove(pluginId);
                _logger.LogInfo($"插件卸载成功: {plugin.Name}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"卸载插件失败: {plugin.Name}", ex);
                return false;
            }
        }

        /// <summary>
        /// 获取已加载的插件
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        public IVisionPlugin GetPlugin(string pluginId)
        {
            _loadedPlugins.TryGetValue(pluginId, out var plugin);
            return plugin;
        }

        /// <summary>
        /// 获取所有已加载的插件
        /// </summary>
        public List<IVisionPlugin> GetAllPlugins()
        {
            return _loadedPlugins.Values.ToList();
        }

        /// <summary>
        /// 获取所有插件的算法节点类型
        /// </summary>
        public List<Type> GetAllAlgorithmNodes()
        {
            var allNodes = new List<Type>();
            
            foreach (var plugin in _loadedPlugins.Values)
            {
                try
                {
                    var nodes = plugin.GetAlgorithmNodes();
                    if (nodes != null)
                    {
                        allNodes.AddRange(nodes);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"获取插件算法节点失败: {plugin.Name}", ex);
                }
            }

            return allNodes;
        }

        /// <summary>
        /// 卸载所有插件
        /// </summary>
        public void UnloadAllPlugins()
        {
            var pluginIds = _loadedPlugins.Keys.ToList();
            foreach (var pluginId in pluginIds)
            {
                UnloadPlugin(pluginId);
            }
        }
    }
}
