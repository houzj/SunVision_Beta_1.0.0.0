using System;
using System.Collections.Generic;
using System.Linq;
using VisionMaster.Interfaces;
using VisionMaster.Models;

namespace VisionMaster.Workflow
{
    /// <summary>
    /// 工作流
    /// </summary>
    public class Workflow
    {
        /// <summary>
        /// 工作流ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 工作流名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 工作流描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 节点列表
        /// </summary>
        public List<WorkflowNode> Nodes { get; private set; }

        /// <summary>
        /// 节点连接关系（源节点ID -> 目标节点ID列表）
        /// </summary>
        public Dictionary<string, List<string>> Connections { get; private set; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        private ILogger Logger { get; set; }

        public Workflow(string id, string name, ILogger logger)
        {
            Id = id;
            Name = name;
            Logger = logger;
            Nodes = new List<WorkflowNode>();
            Connections = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        public void AddNode(WorkflowNode node)
        {
            Nodes.Add(node);
            Logger.LogInfo($"工作流 {Name} 添加节点: {node.Name}");
        }

        /// <summary>
        /// 移除节点
        /// </summary>
        public void RemoveNode(string nodeId)
        {
            var node = Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node != null)
            {
                Nodes.Remove(node);
                Connections.Remove(nodeId);
                
                // 移除其他节点指向该节点的连接
                foreach (var kvp in Connections)
                {
                    kvp.Value.Remove(nodeId);
                }

                Logger.LogInfo($"工作流 {Name} 移除节点: {node.Name}");
            }
        }

        /// <summary>
        /// 连接节点
        /// </summary>
        public void ConnectNodes(string sourceNodeId, string targetNodeId)
        {
            if (!Connections.ContainsKey(sourceNodeId))
            {
                Connections[sourceNodeId] = new List<string>();
            }

            if (!Connections[sourceNodeId].Contains(targetNodeId))
            {
                Connections[sourceNodeId].Add(targetNodeId);
                Logger.LogInfo($"工作流 {Name} 连接节点: {sourceNodeId} -> {targetNodeId}");
            }
        }

        /// <summary>
        /// 断开节点连接
        /// </summary>
        public void DisconnectNodes(string sourceNodeId, string targetNodeId)
        {
            if (Connections.ContainsKey(sourceNodeId))
            {
                Connections[sourceNodeId].Remove(targetNodeId);
                Logger.LogInfo($"工作流 {Name} 断开连接: {sourceNodeId} -> {targetNodeId}");
            }
        }

        /// <summary>
        /// 执行工作流
        /// </summary>
        public List<AlgorithmResult> Execute(Mat inputImage)
        {
            var results = new List<AlgorithmResult>();
            var nodeResults = new Dictionary<string, Mat>();
            var executedNodes = new HashSet<string>();

            Logger.LogInfo($"开始执行工作流: {Name}");

            // 找到所有没有输入连接的节点作为起始节点
            var startNodes = Nodes.Where(n => !Connections.Values.Any(v => v.Contains(n.Id))).ToList();

            foreach (var startNode in startNodes)
            {
                ExecuteNodeRecursive(startNode, inputImage, nodeResults, executedNodes, results);
            }

            Logger.LogInfo($"工作流 {Name} 执行完成，共执行 {executedNodes.Count} 个节点");
            
            return results;
        }

        private void ExecuteNodeRecursive(WorkflowNode node, Mat inputImage, 
            Dictionary<string, Mat> nodeResults, HashSet<string> executedNodes, 
            List<AlgorithmResult> results)
        {
            if (executedNodes.Contains(node.Id) || !node.IsEnabled)
            {
                return;
            }

            executedNodes.Add(node.Id);

            if (node is AlgorithmNode algoNode)
            {
                var result = algoNode.Execute(inputImage);
                results.Add(result);
                nodeResults[node.Id] = (result.ResultImage as Mat) ?? inputImage;
                inputImage = (result.ResultImage as Mat) ?? inputImage;
            }

            // 执行后续节点
            if (Connections.TryGetValue(node.Id, out var targetNodeIds))
            {
                foreach (var targetNodeId in targetNodeIds)
                {
                    var targetNode = Nodes.FirstOrDefault(n => n.Id == targetNodeId);
                    if (targetNode != null)
                    {
                        ExecuteNodeRecursive(targetNode, inputImage, nodeResults, executedNodes, results);
                    }
                }
            }
        }

        /// <summary>
        /// 获取工作流信息
        /// </summary>
        public string GetWorkflowInfo()
        {
            var info = $"工作流: {Name} (ID: {Id})\n";
            info += $"描述: {Description}\n";
            info += $"节点数: {Nodes.Count}\n";
            info += "节点列表:\n";
            
            foreach (var node in Nodes)
            {
                var enabledStatus = node.IsEnabled ? "启用" : "禁用";
                info += $"  - {node.Name} ({node.Id}) [{enabledStatus}]\n";
            }

            return info;
        }
    }
}
