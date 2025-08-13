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
        public ChatData chatData;
        public List<Port> input=new List<Port>();
        public List<Port> output=new List<Port>();

        public List<PortData> inputPortData=new List<PortData>();
        public List<PortData> outputPortData=new List<PortData>();
        //节点GUID
        [SerializeField] private string _GUID;
        public string GUID
        {
            get => _GUID;
            set => _GUID = value;
        }
        //节点类型
        [SerializeField] private NodeType _Type;
        public NodeType Type
        {
            get => _Type;
            set => _Type = value;
        }
        //节点标题
        [SerializeField] private string _Title;
        public string Title
        {
            get => _Title;
            set => _Title = value;
        }
        public virtual void Init(StoryGraphView graphView, string title, Vector2 position,ChatData chatData)
        {
            this.graphView = graphView;
            SetPosition(new Rect(position, Vector2.zero));
            
            Type=NodeType.Base;
            GUID=UnityEditor.GUID.Generate().ToString();
            Title = title;
            this.chatData=chatData;
            InitPort();
            capabilities |= Capabilities.Resizable;
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

        public virtual void DrawOutputContainer()
        {
            foreach (var port in output)
            {
                outputContainer.Add(port);
            }
        }

        protected virtual void DrawInputContainer()
        {
            foreach (var port in input)
            {
                inputContainer.Add(port);
            }
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
           public VisualElement CreatePortData(object userData)
        {
            PortData portData=(PortData)userData;
            VisualElement portContainer = new();
            VisualElement lineContainer = new();
            lineContainer.userData = userData;
            TextField tfdPortName = ElementUtility.CreateTextArea(portData.PortName, null, callback =>
            {
                portData.PortName = callback.newValue;
                OnEditPortName(portData);
            });
            TextField tfdPortCondition = ElementUtility.CreateTextArea(portData.PortCondition, null, callback =>
            {
                portData.PortCondition = callback.newValue;
                OnEditPortName(portData);
            });
            Button btnDelete = ElementUtility.CreateButton("X", () =>
            {
                if (outputPortData.Count == 1)
                {
                    Debug.LogWarning("至少需要一个端口");
                    return;
                }
                outputPortData.Remove(portData);
                foldout.Remove(portContainer);
                OnRemovePortData(portData);

            });
            lineContainer.Add(tfdPortName);
            lineContainer.Add(tfdPortCondition);
            lineContainer.Add(btnDelete);
            portContainer.Add(lineContainer);
            // 添加USS类名
            portContainer.AddClasses
            (
                "foldout-item"
            );
            lineContainer.AddClasses
            (
                "row-container"
            );
            tfdPortName.AddClasses
            (
                "textfield",
                "textfield__quote",
                "row-item__left-center"
            );
            tfdPortCondition.AddClasses
            (
                "textfield",
                "textfield__quote",
                "row-item__left-center"
            );
            btnDelete.AddClasses
            (
                "row-item__right"
            );
            return portContainer;
        }

        public void OnEditPortName(PortData portData)
        {
            foreach (Port port in outputContainer.Children())
            {
                if (port.userData == portData)
                {
                    port.portName = portData.PortName;
                    break;
                }
            }
        }

        public void OnAddPortData(PortData portData)
        {

            Port newPort = this.CreatePort(portData.PortName);
            newPort.userData = portData;
            if(outputContainer.Contains(newPort))return;
            outputContainer.Add(newPort);
        }
        public void OnRemovePortData(PortData portData)
        {
            Port portToRemove = null;
            foreach (Port port in outputContainer.Children())
            {
                if (port.userData == portData)
                {
                    portToRemove = port;
                    break;
                }
            }
            outputContainer.Remove(portToRemove);
        }

        public void InitPort()
        {
            InitInput();
            InitOutput();
        }

        public void InitInput()
        {
            foreach (var inputdata in inputPortData)
            {
                Port newPort = this.CreatePort(inputdata.PortName, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
                input.Add(newPort);
            }
        }
        public void InitOutput()
        {
            foreach (var outputdata in outputPortData)
            {
                Port newPort = this.CreatePort(outputdata.PortName, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
                output.Add(newPort);
            }
        }
    }
}