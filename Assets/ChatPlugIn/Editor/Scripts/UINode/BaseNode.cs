using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{
    public class BaseNode:Node
    {
        protected  StoryGraphView graphView;
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
        //节点备注
        public string Note {get; set;}
        public void Init(StoryGraphView graphView, string title, Vector2 position)
        {
            this.graphView = graphView;
            SetPosition(new Rect(position, Vector2.zero));
            
            Type=NodeType.Base;
            GUID=UnityEditor.GUID.Generate().ToString();
            Title = title;
            Note = "备注信息";
            
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

        private void DrawExtensionContainer()
        {
            customDataContainer = new VisualElement();
            foldout=ElementUtility.CreateFoldout("节点信息");
            TextField tfdNote = ElementUtility.CreateTextField(Note, "备注", callback =>
            {
                Note = callback.newValue;
            });
            titleContainer.Insert(0,tfdNote);
            foldout.Add(tfdNote);
            customDataContainer.Add(foldout);
            extensionContainer.Add(customDataContainer);
            
            RefreshExpandedState();
        }

        private void DrawOutputContainer()
        {
            output = this.CreatePort("下个节点"); 
            outputContainer.Add(output);
        }

        private void DrawInputContainer()
        {
            input = this.CreatePort("上个节点",Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(input);
        }

        private void DrawTopContainer()
        {
        }

        private void DrawTitleButtonContainer()
        {
        }

        private void DrawTitleContainer()
        {
            TextField tfdTitle = ElementUtility.CreateTextField(Title, "标题", callback =>
            {
                Title = callback.newValue;
            });
            titleContainer.Insert(0,tfdTitle);
        }

        private void DrawMainContainer()
        {
        }
    }
}