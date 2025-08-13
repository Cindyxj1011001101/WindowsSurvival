using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ChatPlugIn
{
    public class NodeCreationBox:ScriptableObject,ISearchWindowProvider
    {
        private StoryGraphView graphView;
        private Texture2D indentationIcon;

        public void Init(StoryGraphView viewer)
        {
             graphView = viewer;
             indentationIcon = new Texture2D(1, 1);
             indentationIcon.SetPixel(0, 0, Color.clear);
             indentationIcon.Apply();
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new()
            {
                new SearchTreeGroupEntry(new GUIContent("添加节点")),
                new SearchTreeEntry(new GUIContent("对话",indentationIcon))
                {
                    level = 1,
                    userData = NodeType.Dialogue
                },
                new SearchTreeEntry(new GUIContent("分支条件",indentationIcon))
                {
                    level = 1,
                    userData = NodeType.BranchCondition
                },
                new SearchTreeEntry(new GUIContent("通过条件",indentationIcon))
                {
                    level = 1,
                    userData = NodeType.PassCondition
                },
                new SearchTreeEntry(new GUIContent("开始",indentationIcon))
                {
                    level = 1,
                    userData = NodeType.Start
                },
                new SearchTreeEntry(new GUIContent("结束",indentationIcon))
                {
                    level = 1,
                    userData = NodeType.End
                },
                new SearchTreeEntry(new GUIContent("选项",indentationIcon))
                {
                    level = 1,
                    userData = NodeType.Choose
                },
            };
            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition);
            NodeType  type=(NodeType)SearchTreeEntry.userData;
            switch (type)
            {
                case NodeType.Start:
                case NodeType.End: 
                case NodeType.BranchCondition:
                case NodeType.PassCondition:
                case NodeType.Dialogue:
                case NodeType.Choose:
                    graphView.CreateNode(SearchTreeEntry.content.text, type, localMousePosition,new ChatData());
                    return true;
                default:
                    return false;
            }
        }
    }
}