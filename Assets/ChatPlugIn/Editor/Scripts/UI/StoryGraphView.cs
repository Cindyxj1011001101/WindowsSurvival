using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{
    public class StoryGraphView : GraphView
    {
        //关联窗口
        private StoryEditorWindow storyEditorWindow;
        private NodeCreationBox nodeCreationBox;
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

        public void CreateNodeFromJson(Type type, Vector2 position, string json)
        {
            BaseNode node = Activator.CreateInstance(type) as BaseNode;
            node.Init(this, "title", position);
            node.Deserialize(json);
            node.Draw();
            AddElement(node);
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
                
        // 创建连线
        public BaseEdge CreateEdge(Port outputPort, Port inputPort)
        {
            BaseEdge edge = new BaseEdge
            {
                output = outputPort,
                input = inputPort
            };
            edge.output.Connect(edge);
            edge.input.Connect(edge);
            
            AddElement(edge);
            
            // 通知节点连接事件
            if (edge.output.node is BaseNode outputNode && edge.input.node is BaseNode inputNode)
            {
                // 通知输入节点它已连接到输出节点
                inputNode.OnConnectedFrom(outputNode);
                // 通知输出节点它已连接到输入节点
                outputNode.OnConnectedTo(inputNode);
            }
            
            return edge;
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
            
            // 通知节点断开连接事件
            foreach (BaseEdge edge in edgesToDelete)
            {
                if (edge.output.node is BaseNode outputNode && edge.input.node is BaseNode inputNode)
                {
                    inputNode.OnDisconnectedFrom(outputNode);
                    outputNode.OnDisconnectedTo(inputNode);
                }
            }
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
            string json= GraphJsonSerializer.SerializeGraph(this, filename);
            
            // 保存文件
            string filePath =directoryPath+"/"+filename + ".json";
            File.WriteAllText(filePath, json);
            
            // 刷新资源数据库
            AssetDatabase.Refresh();
            
            Debug.Log($"对话图已保存到: {filePath}");
        }
        
        // 加载图数据
        public void LoadGraph(string filename)
        {
            string filePath =directoryPath+"/"+filename + ".json";
            
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"找不到文件: {filePath}");
                return;
            }
            // 尝试以文本形式读取
            string fileContent = File.ReadAllText(filePath);
            GraphJsonSerializer.DeserializeGraph(this, fileContent, CreateNodeFromJson);
        }
        
        // 清空图
        public void ClearGraph(string filename)
        {
            SaveGraph(filename);
            // 删除所有节点和连线
            List<GraphElement> elementsToDelete = graphElements.ToList();
            DeleteElements(elementsToDelete);
        }
        #endregion
    }
}

