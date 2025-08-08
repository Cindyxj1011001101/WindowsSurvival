using System.Collections.Generic;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{
    public class DialogueNode:SingleInSingleOutNode
    {
        public string RoleName {get;set; }
        public List<SentenceData> SentenceDatas {get;set; }

        public override void Init(StoryGraphView graphView, string title, Vector2 position)
        {
            base.Init(graphView, title, position);
            Type = NodeType.Dialogue;
            RoleName = "角色名称";
            SentenceDatas = new()
            {
                new SentenceData(RoleEnum.NPC, "发言内容", 0)
            };
        }
        protected override void DrawExtensionContainer()
        {
            customDataContainer = new(); 
            foldout=ElementUtility.CreateFoldout("节点信息");
            // 创建角色信息容器
            VisualElement roleInfoRowContainer = new();
            VisualElement roleInfoColContainer = new();

            // 放置UI元素
            roleInfoRowContainer.Add(roleInfoColContainer);
            foldout.Add(roleInfoRowContainer);
            Button btnAdd=ElementUtility.CreateButton("添加对话", () =>
            {
                SentenceData sentenceData = new SentenceData(RoleEnum.NPC, "对话内容", 0);
                SentenceDatas.Add(sentenceData);
                VisualElement lineContainer = CreateSentenceData(sentenceData);
                foldout.Add(lineContainer);
            });
            foldout.Add(btnAdd);
            customDataContainer.Add(foldout);
            extensionContainer.Add(customDataContainer);
            // 添加USS类名
            customDataContainer.AddClasses
            (
                "node__custom-data-container"
            );
            roleInfoRowContainer.AddClasses
            (
                "row-container",
                "foldout-item"
            );
            roleInfoColContainer.AddClasses
            (
                "col-container",
                //"row-item__left-center",
                "full-width"
            );
            btnAdd.AddClasses
            (
                "foldout-item"
            );

            foreach (SentenceData sentenceData in SentenceDatas)
            {
                VisualElement lineContainer = CreateSentenceData(sentenceData);
                foldout.Add(lineContainer);
            }
            

            RefreshExpandedState();
        }

        private VisualElement CreateSentenceData(object userData)
        {
            SentenceData sentenceData = userData as SentenceData;
            VisualElement lineContainer = new ();
            lineContainer.userData = userData;
            DropdownField RoleDropdown = ElementUtility.CreateEnumDropdown<RoleEnum>(sentenceData.Role, "角色",callback =>
                {
                    if (System.Enum.TryParse<RoleEnum>(callback.newValue, out RoleEnum role))
                    {
                        sentenceData.Role = role;
                    }
                    else
                    {
                        Debug.LogWarning("返回的角色不存在");
                    }
                });
            TextField tfdSentence = ElementUtility.CreateTextArea(sentenceData.Text, "对话文本", callback =>
            {
                sentenceData.Text = callback.newValue;
            });
            FloatField tfdDelay = ElementUtility.CreateFloatField(sentenceData.WaitTime, "延迟时间", callback =>
            {
                sentenceData.WaitTime = callback.newValue;
            });
            Button btnDelete = ElementUtility.CreateButton("X", () =>
            {
                if (SentenceDatas.Count == 1)
                {
                    Debug.LogWarning("至少保留一条对话");
                    return;
                }

                SentenceDatas.Remove(sentenceData);
                foldout.Remove(lineContainer);
            });
            lineContainer.Add(RoleDropdown);
            lineContainer.Add(tfdSentence);
            lineContainer.Add(tfdDelay);
            lineContainer.Add(btnDelete);
            btnDelete.AddClasses
            (
                "row-item__right"
            );
            return lineContainer;
        }
    }
}