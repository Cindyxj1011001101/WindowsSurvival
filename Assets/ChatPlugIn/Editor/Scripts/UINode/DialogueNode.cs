using System;
using System.Collections.Generic;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{

    [Serializable]
    public class DialogueNode:SingleInSingleOutNode
    {
        public ChatData chatData;

        public override void Init(StoryGraphView graphView, string title, Vector2 position)
        {
            if(inputPortData==null)inputPortData=new PortData("输入","条件");
            if(outputPortData.Count==0)outputPortData.Add(new PortData("输出","条件"));
            base.Init(graphView, title, position);
            Type = NodeType.Dialogue;
            chatData = new ChatData(MessageSenderEnum.NPC, "对话内容", "条件", 0, 0, "效果");

        }
        protected override void DrawExtensionContainer()
        {
            customDataContainer = new(); 
            foldout=ElementUtility.CreateFoldout("节点信息");
            // 创建角色信息容器
            VisualElement lineContainer = new ();
            lineContainer.userData = userData;
            DropdownField RoleDropdown = ElementUtility.CreateEnumDropdown<MessageSenderEnum>(chatData.MessageSender, "角色",callback =>
            {
                if (System.Enum.TryParse<MessageSenderEnum>(callback.newValue, out MessageSenderEnum role))
                {
                    chatData.MessageSender= role;
                }
                else
                {
                    Debug.LogWarning("返回的角色不存在");
                }
            });
            TextField tfdSentence = ElementUtility.CreateTextArea(chatData.Message, "对话文本", callback =>
            {
                chatData.Message = callback.newValue;
            });
            //对话条件显示为文本框+内容
            TextField tfdCondition = ElementUtility.CreateTextArea(chatData.MessageCondition, "触发对话条件", callback =>
            {
                chatData.MessageCondition = callback.newValue;
            });
            FloatField tfdPreDelay = ElementUtility.CreateFloatField(chatData.preWaitTime, "触发前延迟时间", callback =>
            {
                chatData.preWaitTime = callback.newValue;
            });
            FloatField tfdLateDelay = ElementUtility.CreateFloatField(chatData.lateWaitTime, "触发后延迟时间", callback =>
            {
                chatData.lateWaitTime = callback.newValue;
            });
            TextField tfdEffect = ElementUtility.CreateTextArea(chatData.TriggerMessageEffect, "对话效果", callback =>
            {
                chatData.TriggerMessageEffect = callback.newValue;
            });
            lineContainer.Add(RoleDropdown);
            lineContainer.Add(tfdSentence);
            lineContainer.Add(tfdCondition);
            lineContainer.Add(tfdPreDelay);
            lineContainer.Add(tfdLateDelay);
            lineContainer.Add(tfdEffect);
            foldout.Add(lineContainer);
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