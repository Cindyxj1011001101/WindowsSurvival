using System;
using System.Collections.Generic;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{
    [Serializable]
    public class StartNode : ZeroInSingleOutNode
    {
        public ParagraphData paragraphData;
        public override void Init(StoryGraphView graphView, string title, Vector2 position)
        {
            if(outputPortData.Count==0)outputPortData.Add(new PortData("输出","条件"));
            base.Init(graphView, title, position);
            Type = NodeType.Start;
            paragraphData = new ParagraphData("Start", 0, "条件");
        }
        protected override void DrawExtensionContainer()
        {
            customDataContainer = new VisualElement();
            foldout=ElementUtility.CreateFoldout("节点信息");
            TextField Name= ElementUtility.CreateTextField(paragraphData.ParagraphName, "段落名称", callback =>
            {
                paragraphData.ParagraphName = callback.newValue;
            });
            TextField Condition= ElementUtility.CreateTextField(paragraphData.ParagraphCondition, "触发条件", callback =>
            {
                paragraphData.ParagraphCondition = callback.newValue;
            });
            FloatField Priority= ElementUtility.CreateFloatField(paragraphData.ParagraphPriority, "优先级", callback =>
            {
                paragraphData.ParagraphPriority = callback.newValue;
            });
            foldout.Add(Name);
            foldout.Add(Condition);
            foldout.Add(Priority);
            customDataContainer.Add(foldout);
            extensionContainer.Add(customDataContainer);
            // 添加USS类名
            customDataContainer.AddClasses
            (
                "node__custom-data-container"
            );
            RefreshExpandedState();
        }
    }
}