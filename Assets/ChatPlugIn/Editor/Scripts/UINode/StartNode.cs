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
        public ParagraphData paragraphData=new ParagraphData();
        public override void Init(StoryGraphView graphView, string title, Vector2 position,ChatData chatData)
        {
            if(outputPortData.Count==0)outputPortData.Add(new PortData("输出"));
            base.Init(graphView, title, position,chatData);
            Type = NodeType.Start;
        }
        protected override void DrawExtensionContainer()
        {
            customDataContainer = new VisualElement();
            foldout=ElementUtility.CreateFoldout("节点信息");
            Debug.Log(paragraphData.TriggerParagraphCondition);
            TextField Condition= ElementUtility.CreateTextField(paragraphData.TriggerParagraphCondition, "触发条件", callback =>
            {
                paragraphData.TriggerParagraphCondition = callback.newValue;
            });
            IntegerField Priority= ElementUtility.CreateIntField(paragraphData.ParagraphPriority, "优先级", callback =>
            {
                paragraphData.ParagraphPriority = callback.newValue;
            });
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