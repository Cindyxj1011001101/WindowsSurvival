using System;
using System.Collections.Generic;
using System.Linq;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChatPlugIn
{
    [Serializable]
    public class ChooseNode:SingleInMulti0utNode
    {
        public override void Init(StoryGraphView graphView, string title, Vector2 position)
        {
            if(inputPortData.Count==0) inputPortData.Add(new PortData("输入"));
            base.Init(graphView, title, position);
            Type = NodeType.Choose;
        }
      
    }
}