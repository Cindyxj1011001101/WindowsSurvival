using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class TechNodeProgressData
{
    public string name;
    public float progress;
    public float cost;
    public bool Studied => progress >= cost;

    public TechNodeProgressData() { }

    public TechNodeProgressData(string name, float cost)
    {
        this.name = name;
        this.cost = cost;
        this.progress = 0f;
    }
}

public class TechnologyData
{
    public List<string> studyQueue = new(); // 待研究节点队列
    public Dictionary<string, TechNodeProgressData> techNodeProgressDict = new();
}