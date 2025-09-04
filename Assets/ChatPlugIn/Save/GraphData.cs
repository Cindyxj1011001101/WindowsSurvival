using System;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Input,
    Output
}

    [Serializable]
    public class GraphData:ScriptableObject
    {
        [Serializable]
        public class SerializedNode
        {
            public string guid;
            public string Title;
            public string typeName;
            public Vector2 NodePos;
            public ChatData chatData;
            public SerializedPort inputport = new SerializedPort();
            public List<SerializedPort> outputports = new List<SerializedPort>();
        }

        [Serializable]
        public class SerializedPort
        {
            public string name;
            public Direction direction;
            public string PortCondition;
        }

        [Serializable]
        public class SerializedEdge
        {
            public string outputNodeGUID;
            public string outputPortName;
            public string inputNodeGUID;
            public string inputPortName;
        }
        public ParagraphData paragraphData;
        public List<SerializedNode> nodes = new List<SerializedNode>();
        public List<SerializedEdge> edges = new List<SerializedEdge>();
    }