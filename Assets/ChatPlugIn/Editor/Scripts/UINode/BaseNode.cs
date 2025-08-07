using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{
    [Serializable]
    public class BaseNode:Node
    {
        protected StoryGraphView graphView;
        protected VisualElement customDataContainer;
        protected Foldout foldout;
        protected Port input;
        protected Port output;

        //节点GUID
        public string GUID {get;set;}
        //节点类型
        public NodeType Type{get;set;}
        //节点标题
        public string Title { get; set; }
        public List<ChoiceData> ChoiceDatas;
        public List<BaseNode> InputNodes=new List<BaseNode>();
        public List<BaseNode> OutputNodes=new List<BaseNode>();
        public virtual void Init(StoryGraphView graphView, string title, Vector2 position)
        {
            this.graphView = graphView;
            SetPosition(new Rect(position, Vector2.zero));
            
            Type=NodeType.Base;
            GUID=UnityEditor.GUID.Generate().ToString();
            Title = title;
            ChoiceDatas = new() { new("下个节点") };
            //添加USS类名
            mainContainer.AddToClassList("node__main-container");
            titleContainer.AddToClassList("node__title-container");
            inputContainer.AddToClassList("node__input-container");
            outputContainer.AddToClassList("node__output-container");
            extensionContainer.AddToClassList("node__extension-container");
        }
        // 绘制上下文菜单
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("清除输入连接",
                action => DisconnectedInputPorts(),
                HasInputConnection() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendAction("清除输出连接",
                action => DisconnectedOutputPorts(),
                HasOutputConnection() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendAction("清除所有连接",
                action => DisconnectedAllPorts(),
                HasAnyConnection() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendSeparator();
        }

        public void OnConnectedFrom(BaseNode node)
        {
            Debug.Log("OnConnectedFrom");
            InputNodes.Add(node);
        }
        public void OnConnectedTo(BaseNode node)
        {
            Debug.Log("OnConnectedTo");
            OutputNodes.Add(node);
        }

        public void OnDisconnectedFrom(BaseNode node)
        {
            Debug.Log("OnDisConnectedFrom");
            InputNodes.Remove(node);
        }
        
        public void OnDisconnectedTo(BaseNode node)
        {
            Debug.Log("OnDisconnectedTo");
            OutputNodes.Remove(node);
        }
        
        public virtual void Draw()
        {
            DrawMainContainer();
            DrawTitleContainer();
            DrawTitleButtonContainer();
            DrawTopContainer();
            DrawInputContainer();
            DrawOutputContainer();
            DrawExtensionContainer();
        }

        protected virtual void DrawExtensionContainer()
        {
            customDataContainer = new VisualElement();
            foldout=ElementUtility.CreateFoldout("节点信息");
            customDataContainer.Add(foldout);
            extensionContainer.Add(customDataContainer);
            // 添加USS类名
            customDataContainer.AddClasses
            (
                "node__custom-data-container"
            );
            RefreshExpandedState();
        }

        protected virtual void DrawOutputContainer()
        {
            foreach (var choiceData in ChoiceDatas)
            {
                output = this.CreatePort(choiceData.Text);
                output.userData = choiceData;
                outputContainer.Add(output);
            }

        }

        protected virtual void DrawInputContainer()
        {
            input = this.CreatePort("上个节点",Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(input);
        }

        protected virtual void DrawTopContainer()
        {
        }

        protected virtual void DrawTitleButtonContainer()
        {
        }

        protected virtual void DrawTitleContainer()
        {
            TextField tfdTitle = ElementUtility.CreateTextField(Title, null, callback =>
            {
                Title = callback.newValue;
            });
            titleContainer.Insert(0,tfdTitle);
            // 添加USS类名
            tfdTitle.AddClasses
            (
                "textfield",
                "textfield__hidden",
                "textfield__node-title"
            );
        }

        protected virtual void DrawMainContainer()
        {
        }
        // 是否有任何连接
        public bool HasAnyConnection()
        {
            return HasInputConnection() || HasOutputConnection();
        }

        // 是否有上行连接
        public bool HasInputConnection()
        {
            if (inputContainer.childCount == 0)
            {
                return false;
            }

            Port port = (Port)inputContainer.Children().First();
            return port.connected;
        }

        // 是否有下行连接
        public bool HasOutputConnection()
        {
            if (outputContainer.childCount == 0)
            {
                return false;
            }

            foreach (Port port in outputContainer.Children().ToList())
            {
                if (port.connected)
                {
                    return true;
                }
            }

            return false;
        }

        // 断开所有连接
        public void DisconnectedAllPorts()
        {
            DisconnectedInputPorts();
            DisconnectedOutputPorts();
        }

        // 断开输入连接
        private void DisconnectedInputPorts()
        {
            DisconnectPorts(inputContainer,true);
        }

        // 断开输出连接
        private void DisconnectedOutputPorts()
        {
            DisconnectPorts(outputContainer,false);
        }

        // 断开目标端口连接
        private void DisconnectPorts(VisualElement container,bool isInput)
        {
            foreach (Port port in container.Children())
            {
                DisconnectPort(port,isInput);
            }
        }

        // 断开目标端口连接
        protected void DisconnectPort(Port port,bool isInput)
        {
            if (port.connected)
            {
                graphView.DeleteElements(port.connections.ToList());
            }
        }
        protected virtual void OnEnable()
        {
            if (string.IsNullOrEmpty(GUID))
            {
                GUID = Guid.NewGuid().ToString();
            }
        }
        // 返回节点特定的可序列化数据
        public virtual string Serialize()
        {
            return JsonUtility.ToJson(this);
        }
    
        // 从JSON加载节点数据
        public virtual void Deserialize(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}