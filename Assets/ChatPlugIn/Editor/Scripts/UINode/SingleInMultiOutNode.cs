using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{
    public class SingleInMulti0utNode:BaseNode
    {
        public override void Init(StoryGraphView graphView, string title, Vector2 position)
        {
            base.Init(graphView, title, position);
            Type=NodeType.SingleInMulti0ut;
            ChoiceDatas.Clear();
            ChoiceDatas.Add(new ("选项文本"));
        }
        protected override void DrawOutputContainer()
        {
            foreach (ChoiceData choiceData in ChoiceDatas)
            {
                output = CreateOutputPort(choiceData);
                outputContainer.Add(output);
            }
        }
        protected override void DrawExtensionContainer()
        { 
            Button btnAdd = ElementUtility.CreateButton("添加选项", () =>
            {
                ChoiceData ChoiceData = new ("选项文本");
                ChoiceDatas.Add(ChoiceData);
                output = CreateOutputPort(ChoiceData);
                outputContainer.Add(output);
            });
            extensionContainer.Add(btnAdd);
            // 添加USS类名
            btnAdd.AddClasses
            (
                "foldout-item"
            );
            
            RefreshExpandedState();
        }
        private Port CreateOutputPort(object userData)
        {
            ChoiceData choiceData=(ChoiceData)userData;
            Port outputPort = this.CreatePort();
            Button btnDelete=ElementUtility.CreateButton("X",()=>
            {
                if (ChoiceDatas.Count == 1)
                {
                    Debug.LogWarning("需至少保留一条选项");
                    return;
                }
                ChoiceDatas.Remove(choiceData);
                outputContainer.Remove(outputPort);
            });
            TextField tfdChoice = ElementUtility.CreateTextField(choiceData.Text, null, callback =>
            {
                choiceData.Text = callback.newValue;
            });
            tfdChoice.style.width = 100;
            outputPort.Add(btnDelete);
            outputPort.Add(tfdChoice);
            
            // 添加USS类名
            btnDelete.AddClasses
            (
                "row-item__right"
            );
            tfdChoice.AddClasses
            (
                "textfield",
                "textfield__node-output-port",
                "textfield__hidden"
            );
            
            return outputPort;
        }
    }
}