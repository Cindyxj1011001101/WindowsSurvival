using System.Collections.Generic;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{
    public class EndNode : SingleInZero0utNode
    {
        public override void Init(StoryGraphView graphView, string title, Vector2 position)
        {
            base.Init(graphView, title, position);
            Type = NodeType.End;

        }
    }
}