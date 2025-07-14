using System.Collections.Generic;

public class TechnologyData
{
    public float basicStudyRate = 2.0f; // 基础研究速率，即每15分钟增长多少科技点

    public List<string> studiedTechNodes = new(); // 学习过的科技节点

    public string curStudiedTechNodeName; // 当前正在学习的科技节点
    public float curProgress; // 当前学习进度
    public float curStudyRate; // 当前学习速度
}