using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class StudyProgressData
{
    public string techName;
    public float progress;
    public float cost;
    public bool Complished => progress >= cost;

    public StudyProgressData() { }

    public StudyProgressData(string techName, float cost)
    {
        this.techName = techName;
        this.cost = cost;
        this.progress = 0f;
    }

    public void AddProgress(float value)
    {
        progress = Math.Min(progress + value, cost);
    }
}

public class TechnologyData
{
    public bool isStudying;
    public List<string> studyQueue = new(); // 待研究节点队列
    public Dictionary<string, StudyProgressData> studyProgressDict = new();
}