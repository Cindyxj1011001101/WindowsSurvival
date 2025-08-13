using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{
    [Serializable]
    public class ZeroInSingleOutNode : BaseNode
    {
        public override void Init(StoryGraphView graphView, string title, Vector2 position,ChatData chatData)
        {
            base.Init(graphView, title, position,chatData);
            Type = NodeType.ZeroInSingleOut;
        }
        public override void Draw()
        {
            DrawMainContainer();
            DrawTitleContainer();
            DrawTitleButtonContainer();
            DrawTopContainer();
            DrawOutputContainer();
            DrawExtensionContainer();
        }
    }
}