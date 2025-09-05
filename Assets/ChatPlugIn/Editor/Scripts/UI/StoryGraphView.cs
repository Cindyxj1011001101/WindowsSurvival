using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{
    public class StoryGraphView : GraphView
    {
        //关联窗口
        private StoryEditorWindow storyEditorWindow;
        private NodeCreationBox nodeCreationBox;
        private GraphData graphData;
        private List<Edge> Edges => edges.ToList();
        private List<BaseNode> Nodes => nodes.ToList().Cast<BaseNode>().ToList();
        //构造器
        public StoryGraphView(StoryEditorWindow storyEditorWindow)
        {
            
            this.storyEditorWindow = storyEditorWindow;
            AddGridBackground();
            AddManipulators();
            AddNodeCreationBox();
            OnOpenNodeCreationBox();
        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            // 获取所有端口
            List<Port> result = ports.ToList();
            // 执行筛选
            result = result.Where
            (
                // 两个端口的逻辑方向不能相同（即数据流向输入输出不能相同）
                endport => endport.direction != startPort.direction
                           // 两个端口不能为同一个端口
                           && endport.node != startPort.node
            ).ToList();

            return result;
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            evt.menu.AppendAction("添加节点", action =>
            {
                Vector2 screenMousePosition=action.eventInfo.mousePosition+storyEditorWindow.position.position+new Vector2(50,35);
                nodeCreationRequest(new NodeCreationContext
                {
                    screenMousePosition = screenMousePosition,
                    index = -1
                });
            });
        }
        //添加网格背景 
        private void AddGridBackground()
        {
            //实例化网格背景
             GridBackground gridBackground = new GridBackground();
             //将网格背景尺寸 拉伸至与视图相同
             gridBackground.StretchToParentSize();
             //将网格背景插入视图最底层
             Insert(0, gridBackground);
        }
        //添加视图操作组件
        private void AddManipulators()
        {
            //添加视图缩放组件
            this.AddManipulator(new ContentZoomer());
            SetupZoom(0.2f,2.0f);
            //添加视图拖拽组件
            this.AddManipulator(new ContentDragger());
            //添加选中对象拖拽组件
            this.AddManipulator(new SelectionDragger());
            //添加框选组件
            this.AddManipulator(new RectangleSelector());
        }

        public BaseNode CreateNodeFromSO(GraphData.SerializedNode nodeData)
        {
            Type nodeType = Type.GetType($"ChatPlugIn.{nodeData.typeName}Node");
            BaseNode node = Activator.CreateInstance(nodeType) as BaseNode;
            node.InitPortData(nodeData.inputport);
            node.InitPortData(nodeData.outputports);
            if (nodeData.typeName == "Start")
            {
                ((StartNode)node).paragraphData=graphData.paragraphData;
            }
            node.Init(this,nodeData.Title,nodeData.NodePos);
            node.GUID = nodeData.guid;
            if(nodeType==typeof(DialogueNode))
                (node as DialogueNode).chatData=nodeData.chatData;
            node.Draw();
            AddElement(node);
            return node;
        }
        public BaseNode CreateNode(string title,NodeType type, Vector2 position)
        {
            Type nodeType = Type.GetType($"ChatPlugIn.{type}Node");
            BaseNode node = Activator.CreateInstance(nodeType) as BaseNode;
            node.Init(this, title, position);
            node.Draw();
            AddElement(node);
            return node;
        }
        
        public new void DeleteElements(IEnumerable<GraphElement> elements)
        {
            // 记录将要删除的连接，以便通知节点
            List<Edge> edgesToDelete = new List<Edge>();
            
            foreach (GraphElement element in elements)
            {
                if (element is Edge edge)
                {
                    edgesToDelete.Add(edge);
                }
            }
            // 先调用基类方法删除元素
            base.DeleteElements(elements);

        }

        #region CreationBox

        private void AddNodeCreationBox()
        {
            nodeCreationBox = ScriptableObject.CreateInstance<NodeCreationBox>();
            nodeCreationBox.Init(this);
        }

        private void OnOpenNodeCreationBox()
        {
            nodeCreationRequest = context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), nodeCreationBox);
            };
        }

        #endregion
        #region 坐标转换

        public Vector2 GetLocalMousePosition(Vector2 screenMousePosition)
        {
            Vector2 windowMousePosition=screenMousePosition-storyEditorWindow.position.position;
            Vector2 localMousePosition = contentViewContainer.WorldToLocal(windowMousePosition);
            return localMousePosition;
        }

        #endregion

        #region 保存相关功能实现
        // 清空图
        public void ClearGraph(string filename)
        {
            // SaveGraph(filename);
            // 删除所有节点和连线
            List<GraphElement> elementsToDelete = graphElements.ToList();
            DeleteElements(elementsToDelete);
        }
        public void SaveGraph(string fileName)
        {
            var graph = ScriptableObject.CreateInstance<GraphData>();
            
            // 保存连接数据
            var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
            graph.edges = new List<GraphData.SerializedEdge>();
            
            for (int i = 0; i < connectedPorts.Length; i++)
            {
                var outputNode = connectedPorts[i].output.node as BaseNode;
                var inputNode = connectedPorts[i].input.node as BaseNode;
                
                graph.edges.Add(new GraphData.SerializedEdge
                {
                    outputNodeGUID = outputNode.GUID,
                    outputPortName = connectedPorts[i].output.portName,
                    inputNodeGUID = inputNode.GUID,
                    inputPortName = connectedPorts[i].input.portName
                });
            }
            
            // 保存节点数据
            graph.nodes =  new List<GraphData.SerializedNode>();
            for (int i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                GraphData.SerializedNode nodeData = new GraphData.SerializedNode()
                {
                    Title = node.Title,
                    guid = node.GUID,
                    typeName = node.Type.ToString(),
                    NodePos = node.GetPosition().position,
                };
                if (node.Type == NodeType.Start)
                {
                    graph.paragraphData = ((StartNode)node).paragraphData;
                }
                if (node.Type == NodeType.Dialogue)
                {
                    nodeData.chatData = ((DialogueNode)node).chatData;
                }

                if(node.inputPortData==null) nodeData.inputport=null;
                else
                {
                    nodeData.inputport=new GraphData.SerializedPort()
                    {
                        name = node.inputPortData.PortName,
                        direction = Direction.Input,
                        PortCondition =node.inputPortData.PortCondition
                        
                    };
                }

                foreach (var portData in node.outputPortData)
                {
                    nodeData.outputports.Add(new GraphData.SerializedPort()
                    {
                        name = portData.PortName,
                        direction = Direction.Output,
                        PortCondition = portData.PortCondition
                        
                    });
                }

                graph.nodes.Add(nodeData);
            }
            Debug.Log("保存成功");
            // 确保目录存在
            if (!Directory.Exists("Assets/Resources/DialogueGraphs"))
                Directory.CreateDirectory("Assets/Resources/DialogueGraphs");
            
            // 保存Asset
            AssetDatabase.CreateAsset(graph, $"Assets/Resources/DialogueGraphs/{fileName}.asset");
            AssetDatabase.SaveAssets();
        }

        public void LoadGraph(string fileName)
        {
            graphData = Resources.Load<GraphData>($"DialogueGraphs/{fileName}");
            if (graphData == null)
            {
                EditorUtility.DisplayDialog("File Not Found", "Target dialogue graph does not exist!", "OK");
                return;
            }
            
            ClearGraph(fileName);
            CreateNodes();
            ConnectNodes();
        }
        private void CreateNodes()
        {
            foreach (var nodeData in graphData.nodes)
            {
                var tempNode = CreateNodeFromSO(nodeData);

            }
        }

        private void ConnectNodes()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                var connections = graphData.edges.Where(x => x.outputNodeGUID == Nodes[i].GUID).ToList();
                for (int j = 0; j < connections.Count; j++)
                {
                    var InputNodeGuid = connections[j].inputNodeGUID;
                    var targetNode = Nodes.First(x => x.GUID == InputNodeGuid);
                    foreach (var port in Nodes[i].outputContainer.Children())
                    {
                        if (port.Q<Port>().portName== connections[j].outputPortName)
                        {
                            LinkNodes(port.Q<Port>(), (Port)targetNode.inputContainer[0]);
                            break;
                        }
                    }
                    targetNode.SetPosition(new Rect(
                        graphData.nodes.First(x => x.guid == InputNodeGuid).NodePos,
                        Vector2.zero));
                }
            }
        }

        private void LinkNodes(Port output, Port input)
        {
            var tempEdge = new Edge
            {
                output = output,
                input = input
            };
            
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            Add(tempEdge);
        }
         }
    #endregion
}

