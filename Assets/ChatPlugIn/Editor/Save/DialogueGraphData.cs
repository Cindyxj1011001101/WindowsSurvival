using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChatPlugIn
{
    
    [CreateAssetMenu(fileName = "New Dialogue Graph", menuName = "Dialogue/Dialogue Graph")]
    public class DialogueGraphData : ScriptableObject
    {
        [Serializable]
        public class NodeLinkData
        {
            public string baseNodeGuid;
            public string portName;
            public string targetNodeGuid;
        }

        [Serializable]
        public class DialogueNodeData
        {
            public string GUID;
            public string title;
            public NodeType type;
            public Vector2 position;
            public ChatData chatData;
        }
        public ParagraphData ParagraphData;
        public List<DialogueNodeData> nodeData;
        public List<NodeLinkData> linkData;
    }
}