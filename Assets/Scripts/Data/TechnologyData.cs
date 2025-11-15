using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class TechNodeData
{
    public string name;
    public float progress;
}

public class TechnologyData
{
    public float basicStudyRate;                    // 基础研究速率，即每15分钟增长多少科技点

    public List<string> studiedTechNodes = new();   // 学习过的科技节点

    public string curStudiedTechNodeName;           // 当前正在学习的科技节点

    public bool isIntermediateTechLocked;           // 中级科技是否锁定

    public string intermediateTechLockedReason;     // 中级科技锁定的原因

    public Dictionary<string, TechNodeData> techNodeProgressDict = new();

    [JsonIgnore]
    public TechNodeData CurStudiedTechNodeData => techNodeProgressDict[curStudiedTechNodeName];
}