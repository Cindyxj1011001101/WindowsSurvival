using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ChatPlugIn
{
    [Serializable]
    public class BaseEdge:Edge
    {
        public string outputNodeGUID {get;set;}
        public string outputPortName {get;set;}
        public string inputNodeGUID {get;set;}
        public string inputPortName {get;set;}
        // 返回节点特定的可序列化数据
        public virtual string Serialize()
        {
            return JsonUtility.ToJson(this);
        }
    
        // 从JSON加载节点数据
        public virtual void Deserialize(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}