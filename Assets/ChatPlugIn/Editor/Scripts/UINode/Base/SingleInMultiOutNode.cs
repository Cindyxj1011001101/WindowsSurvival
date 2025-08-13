using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{
    [Serializable]
    public class SingleInMulti0utNode : BaseNode
    {
        private bool hasInited;

        public override void Init(StoryGraphView graphView, string title, Vector2 position,ChatData chatData)
        {
            hasInited = false;
            base.Init(graphView, title, position,chatData);
            Type = NodeType.SingleInMulti0ut;
        }

        protected override void DrawExtensionContainer()
        {
            foldout = ElementUtility.CreateFoldout("节点信息");
            customDataContainer = new();
            Button btnAdd = ElementUtility.CreateButton("添加选项", () =>
            {
                PortData portData = new("选项");
                outputPortData.Add(portData);
                VisualElement lineContainer = CreatePortData(portData);
                foldout.Add(lineContainer);
                OnAddPortData(portData);
                RefreshExpandedState();
            });
            foldout.Add(btnAdd);
            customDataContainer.Add(foldout);
            extensionContainer.Add(customDataContainer);
            foreach (PortData portData in outputPortData)
            {
                VisualElement lineContainer = CreatePortData(portData);
                foldout.Add(lineContainer);
                if (hasInited)
                {
                    OnAddPortData(portData);
                }
            }
            if (!hasInited) hasInited = !hasInited;
            // 添加USS类名
            btnAdd.AddClasses
            (
                "foldout-item"
            );
            customDataContainer.AddClasses
            (
                "node__custom-data-container"
            );
            RefreshExpandedState();
        }
    }
}