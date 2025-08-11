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
        public override void Init(StoryGraphView graphView, string title, Vector2 position)
        {
            base.Init(graphView, title, position);
            Type=NodeType.SingleInSingleOut;
        }
    }
}