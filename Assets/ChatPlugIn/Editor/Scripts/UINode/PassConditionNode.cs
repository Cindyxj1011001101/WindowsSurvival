using System.Collections.Generic;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{
    public class PassConditionNode:SingleInMulti0utNode
    {
        public override void Init(StoryGraphView graphView, string title, Vector2 position)
        {
            base.Init(graphView, title, position);
            Type = NodeType.PassCondition;
            ChoiceDatas.Clear();
            ChoiceDatas.Add(new ("选项文本1"));
            ChoiceDatas.Add(new ("选项文本2"));
        }
        protected override void DrawOutputContainer()
        {
            foreach (ChoiceData choiceData in ChoiceDatas)
            {
                output = this.CreatePort(choiceData.Text);
                output.userData = choiceData;
                outputContainer.Add(output);
            }
        }
        protected override void DrawExtensionContainer()
        {
            customDataContainer = new();
            foldout=ElementUtility.CreateFoldout("节点信息");
            Button btnAdd = ElementUtility.CreateButton("添加选项", () =>
            {
                ChoiceData choiceData = new("选项文本");
                ChoiceDatas.Add(choiceData);
                VisualElement lineContainer = CreateChoiceData(choiceData);
                foldout.Add(lineContainer);
                OnAddChoiceData(choiceData);
            });
            foldout.Add(btnAdd);
            customDataContainer.Add(foldout);
            extensionContainer.Add(customDataContainer);

            foreach (ChoiceData choiceData in ChoiceDatas)
            {
                VisualElement lineContainer = CreateChoiceData(choiceData);
                foldout.Add(lineContainer);

            }
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

        private VisualElement CreateChoiceData(object userData)
        {
            ChoiceData choiceData=(ChoiceData)userData;
            VisualElement choiceContainer = new();
            VisualElement lineContainer = new();
            lineContainer.userData = userData;
            TextField tfdChoice = ElementUtility.CreateTextArea(choiceData.Text, null, callback =>
            {
                choiceData.Text = callback.newValue;
                OnEditChoiceText(choiceData);
            });
            Button btnDelete = ElementUtility.CreateButton("X", () =>
            {
                if (ChoiceDatas.Count == 1)
                {
                    Debug.LogWarning("至少保留一条对话");
                    return;
                }

                ChoiceDatas.Remove(choiceData);
                foldout.Remove(choiceContainer);
                OnRemoveChoiceData(choiceData);

            });
            lineContainer.Add(tfdChoice);
            lineContainer.Add(btnDelete);
            choiceContainer.Add(lineContainer);
            // 添加USS类名
            choiceContainer.AddClasses
            (
                "foldout-item"
            );
            lineContainer.AddClasses
            (
                "row-container"
            );
            tfdChoice.AddClasses
            (
                "textfield",
                "textfield__quote",
                "row-item__left-center"
            );
            btnDelete.AddClasses
            (
                "row-item__right"
            );
            return choiceContainer;
        }

        private void OnEditChoiceText(ChoiceData choiceData)
        {
            foreach (Port port in outputContainer.Children())
            {
                if (port.userData == choiceData)
                {
                    port.portName = choiceData.Text;
                    break;
                }
            }
        }

        private void OnAddChoiceData(ChoiceData choiceData)
        {
            Port newPort = this.CreatePort(choiceData.Text);
            newPort.userData = choiceData;
            outputContainer.Add(newPort);
        }
        private void OnRemoveChoiceData(ChoiceData choiceData)
        {
            Port portToRemove = null;
            foreach (Port port in outputContainer.Children())
            {
                if (port.userData == choiceData)
                {
                    portToRemove = port;
                    break;
                }
            }
            outputContainer.Remove(portToRemove);
        }
    }
}