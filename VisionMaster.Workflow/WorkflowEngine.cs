using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VisionMaster.Interfaces;
using VisionMaster.Models;

namespace VisionMaster.Workflow
{
    /// <summary>
    /// 工作流引擎
    /// </summary>
    public class WorkflowEngine
    {
        /// <summary>
        /// 工作流列表
        /// </summary>
        private Dictionary<string, Workflow> Workflows { get; set; }

        /// <summary>
        /// 当前活动工作流
        /// </summary>
        public Workflow CurrentWorkflow { get; private set; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        private ILogger Logger { get; set; }

        public WorkflowEngine(ILogger logger)
        {
            Logger = logger;
            Workflows = new Dictionary<string, Workflow>();
        }

        /// <summary>
        /// 创建新工作流
        /// </summary>
        public Workflow CreateWorkflow(string id, string name, string description = "")
        {
            if (Workflows.ContainsKey(id))
            {
                throw new ArgumentException($"工作流ID {id} 已存在");
            }

            var workflow = new Workflow(id, name, Logger)
            {
                Description = description
            };

            Workflows[id] = workflow;
            Logger.LogInfo($"创建工作流: {name} (ID: {id})");

            return workflow;
        }

        /// <summary>
        /// 删除工作流
        /// </summary>
        public bool DeleteWorkflow(string id)
        {
            if (Workflows.TryGetValue(id, out var workflow))
            {
                Workflows.Remove(id);
                
                if (CurrentWorkflow?.Id == id)
                {
                    CurrentWorkflow = null;
                }

                Logger.LogInfo($"删除工作流: {workflow.Name} (ID: {id})");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 设置当前工作流
        /// </summary>
        public void SetCurrentWorkflow(string id)
        {
            if (!Workflows.ContainsKey(id))
            {
                throw new ArgumentException($"工作流ID {id} 不存在");
            }

            CurrentWorkflow = Workflows[id];
            Logger.LogInfo($"设置当前工作流: {CurrentWorkflow.Name}");
        }

        /// <summary>
        /// 获取工作流
        /// </summary>
        public Workflow GetWorkflow(string id)
        {
            Workflows.TryGetValue(id, out var workflow);
            return workflow;
        }

        /// <summary>
        /// 获取所有工作流
        /// </summary>
        public List<Workflow> GetAllWorkflows()
        {
            return Workflows.Values.ToList();
        }

        /// <summary>
        /// 执行工作流
        /// </summary>
        public List<AlgorithmResult> ExecuteWorkflow(string workflowId, Mat inputImage)
        {
            var workflow = GetWorkflow(workflowId);
            if (workflow == null)
            {
                Logger.LogError($"工作流不存在: {workflowId}", "Workflow not found");
                return new List<AlgorithmResult> { AlgorithmResult.CreateError($"工作流不存在: {workflowId}") };
            }

            Logger.LogInfo($"执行工作流: {workflow.Name}");
            return workflow.Execute(inputImage);
        }

        /// <summary>
        /// 执行当前工作流
        /// </summary>
        public List<AlgorithmResult> ExecuteCurrentWorkflow(Mat inputImage)
        {
            if (CurrentWorkflow == null)
            {
                Logger.LogError("当前没有活动的工作流", "No active workflow");
                return new List<AlgorithmResult> { AlgorithmResult.CreateError("当前没有活动的工作流") };
            }

            return ExecuteWorkflow(CurrentWorkflow.Id, inputImage);
        }

        /// <summary>
        /// 保存工作流到文件
        /// </summary>
        public void SaveWorkflowToFile(string workflowId, string filePath)
        {
            var workflow = GetWorkflow(workflowId);
            if (workflow == null)
            {
                throw new ArgumentException($"工作流不存在: {workflowId}");
            }

            var workflowData = SerializeWorkflow(workflow);
            var json = JsonSerializer.Serialize(workflowData, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            File.WriteAllText(filePath, json);
            Logger.LogInfo($"工作流已保存到: {filePath}");
        }

        /// <summary>
        /// 从文件加载工作流
        /// </summary>
        public Workflow LoadWorkflowFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"文件不存在: {filePath}");
            }

            var json = File.ReadAllText(filePath);
            var workflowData = JsonSerializer.Deserialize<WorkflowData>(json);

            var workflow = DeserializeWorkflow(workflowData);
            Workflows[workflow.Id] = workflow;
            
            Logger.LogInfo($"工作流已从文件加载: {workflow.Name}");
            
            return workflow;
        }

        private WorkflowData SerializeWorkflow(Workflow workflow)
        {
            return new WorkflowData
            {
                Id = workflow.Id,
                Name = workflow.Name,
                Description = workflow.Description,
                Nodes = workflow.Nodes.Select(n => new NodeData
                {
                    Id = n.Id,
                    Name = n.Name,
                    Type = n.Type.ToString(),
                    IsEnabled = n.IsEnabled,
                    Parameters = n.Parameters.GetAllParameters()
                }).ToList(),
                Connections = workflow.Connections.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value
                )
            };
        }

        private Workflow DeserializeWorkflow(WorkflowData data)
        {
            var workflow = CreateWorkflow(data.Id, data.Name, data.Description);

            foreach (var nodeData in data.Nodes)
            {
                var nodeType = (NodeType)Enum.Parse(typeof(NodeType), nodeData.Type);
                var node = new WorkflowNode(nodeData.Id, nodeData.Name, nodeType)
                {
                    IsEnabled = nodeData.IsEnabled
                };

                foreach (var param in nodeData.Parameters)
                {
                    node.Parameters.SetParameter(param.Key, param.Value);
                }

                workflow.AddNode(node);
            }

            foreach (var connection in data.Connections)
            {
                foreach (var targetId in connection.Value)
                {
                    workflow.ConnectNodes(connection.Key, targetId);
                }
            }

            return workflow;
        }

        private class WorkflowData
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public List<NodeData> Nodes { get; set; }
            public Dictionary<string, List<string>> Connections { get; set; }
        }

        private class NodeData
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public bool IsEnabled { get; set; }
            public Dictionary<string, object> Parameters { get; set; }
        }
    }
}
