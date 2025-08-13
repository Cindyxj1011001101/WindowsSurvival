using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{
    [Serializable]
    public class SingleInSingleOutNode:BaseNode
    {
        public override void Init(StoryGraphView graphView, string title, Vector2 position,ChatData chatData)
        {
            base.Init(graphView, title, position,chatData);
            Type=NodeType.SingleInSingleOut;
        }
    }
}