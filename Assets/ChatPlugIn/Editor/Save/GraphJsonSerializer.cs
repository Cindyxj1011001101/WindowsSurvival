using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{
    public static class GraphJsonSerializer
    {
        public static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public static string SerializeGraph(GraphView graphView, string graphName = "")
        {
            var saveData = new GraphSaveData
            {
                graphViewName = graphName
            };
            // 序列化节点
            foreach (var node in graphView.nodes.ToList().Cast<BaseNode>())
            {
                var serializedNode = new GraphSaveData.SerializedNode
                {
                    guid = node.GUID,
                    typeName = node.GetType().FullName,
                    NodePos = node.GetPosition().position,
                    nodeData = JsonUtility.ToJson(node)
                };

                // 序列化端口
                foreach (var port in node.inputContainer.Children().OfType<Port>())
                {
                    serializedNode.ports.Add(new GraphSaveData.SerializedPort
                    {
                        name = port.portName,
                        portType = port.portType.FullName,
                        direction = Direction.Input,
                        capacity = port.capacity
                    });
                }

                foreach (var port in node.outputContainer.Children().OfType<Port>())
                {
                    serializedNode.ports.Add(new GraphSaveData.SerializedPort
                    {
                        name = port.portName,
                        portType = port.portType.FullName,
                        direction = Direction.Output,
                        capacity = port.capacity
                    });
                }

                saveData.nodes.Add(serializedNode);
            }

            // 序列化边
            foreach (var edge in graphView.edges.ToList())
            {
                var outputNode = edge.output.node as BaseNode;
                var inputNode = edge.input.node as BaseNode;

                saveData.edges.Add(new GraphSaveData.SerializedEdge
                {
                    outputNodeGUID = outputNode.GUID,
                    outputPortName = edge.output.portName,
                    inputNodeGUID = inputNode.GUID,
                    inputPortName = edge.input.portName
                });
            }

            return JsonConvert.SerializeObject(saveData, settings);
        }

        public static void DeserializeGraph(GraphView graphView, string json,
            Func<Type, Vector2, string, BaseNode> nodeCreator)
        {
            GraphSaveData saveData = JsonConvert.DeserializeObject<GraphSaveData>(json, settings);

            // 清空当前视图
            graphView.DeleteElements(graphView.nodes.ToList());
            graphView.DeleteElements(graphView.edges.ToList());

            // 节点GUID到实例的映射
            var nodeMap = new Dictionary<string, BaseNode>();

            // 创建所有节点
            foreach (var nodedata in saveData.nodes)
            {
                var nodeType = Type.GetType(nodedata.typeName);
                if (nodeType == null)
                {
                    Debug.LogError($"无法找到节点类型: {nodedata.typeName}");
                    continue;
                }

                BaseNode node = nodeCreator(nodeType, nodedata.NodePos, nodedata.nodeData);
                JsonUtility.FromJsonOverwrite(nodedata.nodeData, node);
                node.GUID = nodedata.guid;
                node.SetPosition(new Rect(nodedata.NodePos, Vector2.zero));
                graphView.AddElement(node);
                nodeMap.Add(node.GUID, node);
            }

            // 创建所有边
            foreach (var edgeData in saveData.edges)
            {
                if (!nodeMap.TryGetValue(edgeData.outputNodeGUID, out var outputNode) ||
                    !nodeMap.TryGetValue(edgeData.inputNodeGUID, out var inputNode))
                {
                    continue;
                }
                Port outputPort = outputNode.output.FirstOrDefault(x => x.portName == edgeData.outputPortName);
                Port inputPort = inputNode.input.FirstOrDefault(x => x.portName == edgeData.inputPortName);
                if (outputPort != null && inputPort != null)
                {
                    var edge = new BaseEdge()
                    {
                        output = outputPort,
                        input = inputPort
                    };
                    edge.input.Connect(edge);
                    edge.output.Connect(edge);
                    graphView.AddElement(edge);
                }
            }
        }
    }
}