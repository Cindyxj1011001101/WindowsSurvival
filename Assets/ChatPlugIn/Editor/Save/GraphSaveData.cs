using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ChatPlugIn
{
    [Serializable]
    public class GraphSaveData
    {
        [Serializable]
        public class SerializedNode
        {
            public string guid;
            public string typeName;
            public Vector2 NodePos;
            public string nodeData;
            public List<SerializedPort> ports = new List<SerializedPort>();
        }

        [Serializable]
        public class SerializedPort
        {
            public string name;
            public string portType;
            public Direction direction;
            public Port.Capacity capacity;
        }

        [Serializable]
        public class SerializedEdge
        {
            public string outputNodeGUID;
            public string outputPortName;
            public string inputNodeGUID;
            public string inputPortName;
        }
        public string graphViewName;
        public List<SerializedNode> nodes = new List<SerializedNode>();
        public List<SerializedEdge> edges = new List<SerializedEdge>();
        public void Deserialize(string json)
        { 
            JsonConvert.DeserializeObject<GraphSaveData>(json,GraphJsonSerializer.settings);
        }
        public string Serialize()
        {
            return  JsonConvert.SerializeObject(this, GraphJsonSerializer.settings);
        }
    }
}