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
        //图数据
        private DialogueGraphData currentGraph;
        
        string directoryPath = "Assets/Resources/ChatData";
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
        public BaseNode CreateNode(string title,NodeType type, Vector2 position,ChatData chatData)
        {
            Type nodeType = Type.GetType($"ChatPlugIn.{type}Node");
            BaseNode node = Activator.CreateInstance(nodeType) as BaseNode;
            node.Init(this, title, position,chatData);
            node.Draw();
            AddElement(node);
            return node;
        }
        public new void DeleteElements(IEnumerable<GraphElement> elements)
        {
            // 记录将要删除的连接，以便通知节点
            List<BaseEdge> edgesToDelete = new List<BaseEdge>();
            
            foreach (GraphElement element in elements)
            {
                if (element is BaseEdge edge)
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
        public void SaveGraph(string filename)
        {
            var graph = ScriptableObject.CreateInstance<DialogueGraphData>();
            foreach (BaseEdge edge in edges)
            {
                var outputNode =edge.output.node as BaseNode;
                var inputNode = edge.input.node as BaseNode;
                graph.linkData.Add(new DialogueGraphData.NodeLinkData()
                {
                    baseNodeGuid = outputNode.GUID,
                    portName = edge.output.portName,
                    targetNodeGuid = inputNode.GUID
                });
            }
            foreach (BaseNode node in nodes)
            { 
                graph.nodeData.Add(new DialogueGraphData.DialogueNodeData()
                {
                    GUID = node.GUID,
                    position = node.GetPosition().position,
                    type=node.Type,
                    chatData = node.chatData
                });
            }

            // 确保目录存在
            if (!Directory.Exists("Assets/Resources/DialogueGraphs"))
                Directory.CreateDirectory("Assets/Resources/DialogueGraphs");

            // 保存Asset
            AssetDatabase.CreateAsset(graph, $"Assets/Resources/DialogueGraphs/{filename}.asset");
            AssetDatabase.SaveAssets();
        }
        
        // 加载图数据
        public void LoadGraph(string filename)
        {
            currentGraph = Resources.Load<DialogueGraphData>($"DialogueGraphs/{filename}");
            if (currentGraph == null)
            {
                EditorUtility.DisplayDialog("File Not Found", "Target dialogue graph does not exist!", "OK");
                return;
            }
            ClearGraph(filename);
            CreateNodes();
            ConnectNodes();
        }

        private void CreateNodes()
        {
            foreach (var node in currentGraph.nodeData)
            {
                //创建节点
                CreateNode(node.title,node.type,node.position,node.chatData);
            }
        }

        private void ConnectNodes()
        {
            foreach (BaseNode node in nodes)
            {
                var connections = currentGraph.linkData.Where(x => x.baseNodeGuid ==node.GUID).ToList();
                for (int j = 0; j < connections.Count; j++)
                {
                    var targetNodeGuid = connections[j].targetNodeGuid;
                    var targetNode = nodes.First(x => x.guid == targetNodeGuid);

                    LinkNodes(node.outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                    targetNode.SetPosition(new Rect(
                        currentGraph.nodeData.First(x => x.guid == targetNodeGuid).position,
                        targetNode.GetPosition().size));
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
        
        // 清空图
        public void ClearGraph(string filename)
        {
            // SaveGraph(filename);
            // 删除所有节点和连线
            List<GraphElement> elementsToDelete = graphElements.ToList();
            DeleteElements(elementsToDelete);
        }
        #endregion
    }
}

