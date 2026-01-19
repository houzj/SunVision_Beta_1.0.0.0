using System;
using VisionMaster.Models;

namespace VisionMaster.Workflow
{
    /// <summary>
    /// 工作流节点类型
    /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// 算法节点
        /// </summary>
        Algorithm,
        /// <summary>
        /// 输入节点
        /// </summary>
        Input,
        /// <summary>
        /// 输出节点
        /// </summary>
        Output,
        /// <summary>
        /// 条件节点
        /// </summary>
        Condition
    }

    /// <summary>
    /// 工作流节点
    /// </summary>
    public class WorkflowNode
    {
        /// <summary>
        /// 节点ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 节点类型
        /// </summary>
        public NodeType Type { get; set; }

        /// <summary>
        /// 节点参数
        /// </summary>
        public AlgorithmParameters Parameters { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 执行前事件
        /// </summary>
        public event Action<WorkflowNode> BeforeExecute;

        /// <summary>
        /// 执行后事件
        /// </summary>
        public event Action<WorkflowNode, AlgorithmResult> AfterExecute;

        public WorkflowNode(string id, string name, NodeType type)
        {
            Id = id;
            Name = name;
            Type = type;
            Parameters = new AlgorithmParameters();
        }

        /// <summary>
        /// 触发执行前事件
        /// </summary>
        protected virtual void OnBeforeExecute()
        {
            BeforeExecute?.Invoke(this);
        }

        /// <summary>
        /// 触发执行后事件
        /// </summary>
        protected virtual void OnAfterExecute(AlgorithmResult result)
        {
            AfterExecute?.Invoke(this, result);
        }
    }
}
