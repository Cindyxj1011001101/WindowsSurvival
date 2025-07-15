using System.Linq;
using UnityEngine;

public class TechnologyManager
{
    private static TechnologyManager instance = new();
    public static TechnologyManager Instance => instance;

    //private float basicStudyRate = 2.0f; // 基础研究速率，即每15分钟增长多少科技点

    //private List<string> studiedTechNodes = new(); // 学习过的科技节点

    //private string curStudiedTechNodeName; // 当前正在学习的科技节点
    //private float curProgress; // 当前学习进度
    //private float curStudyRate; // 当前学习速度

    private TechnologyData techData;

    public ScriptableTechnologyNode CurStudiedTechNode => Resources.Load<ScriptableTechnologyNode>("ScriptableObject/Technology/" + techData.curStudiedTechNodeName);
    public float CurProgress => techData.curProgress;
    public float CurStudyRate => techData.curStudyRate;

    private TechnologyManager()
    {
        techData = GameDataManager.Instance.TechnologyData;
    }

    /// <summary>
    /// 研究一个科技节点
    /// </summary>
    /// <param name="techNode"></param>
    public void Study(ScriptableTechnologyNode techNode)
    {
        techData.curStudiedTechNodeName = techNode.techName;
        techData.curProgress = 0;
        techData.curStudyRate = CalcStudyRate();
        // 添加监听，每回合结算研究进度
        EventManager.Instance.AddListener(EventType.IntervalSettle, OnStudy);
    }

    private void OnStudy()
    {
        // 计算研究速率
        techData.curStudyRate = CalcStudyRate();
        // 进度增长
        techData.curProgress += techData.curStudyRate;
        // 研究完成
        if (techData.curProgress >= CurStudiedTechNode.cost)
        {
            // 解锁该科技
            UnlockTechNode(CurStudiedTechNode);
            // 设置正在研究的科技节点为空
            techData.curStudiedTechNodeName = null;
            // 设置研究进度为0
            techData.curProgress = 0;
            // 移除监听
            EventManager.Instance.RemoveListener(EventType.IntervalSettle, OnStudy);
        }
        EventManager.Instance.TriggerEvent(EventType.ChangeStudyProgress);
    }

    private float CalcStudyRate()
    {
        return techData.basicStudyRate;
    }

    /// <summary>
    /// 解锁一个科技
    /// </summary>
    private void UnlockTechNode(ScriptableTechnologyNode techNode)
    {
        // 不要重复解锁
        if (techData.studiedTechNodes.Contains(techNode.techName)) return;

        // 将科技节点添加到已解锁列表中
        techData.studiedTechNodes.Add(techNode.techName);

        // 解锁相应配方
        foreach (var recipe in techNode.recipes)
        {
            CraftManager.Instance.UnlockRecipe(recipe);
        }
    }

    /// <summary>
    /// 判断一个科技节点是否锁定
    /// </summary>
    /// <param name="techNode"></param>
    /// <returns></returns>
    public bool IsTechNodeLocked(ScriptableTechnologyNode techNode)
    {
        // 前置条件都满足，则该科技解锁
        return !(techNode.prerequisites.Count == 0 || techNode.prerequisites.All(t => techData.studiedTechNodes.Contains(t.techName)));
    }

    /// <summary>
    /// 判断一个科技节点是否研究完成
    /// </summary>
    /// <param name="techNode"></param>
    /// <returns></returns>
    public bool IsTechNodeStudied(ScriptableTechnologyNode techNode)
    {
        return techData.studiedTechNodes.Contains(techNode.techName);
    }

    /// <summary>
    /// 判断是否有科技节点正在被研究
    /// </summary>
    /// <returns></returns>
    public bool IsAnyTechNodeBeingStudied()
    {
        return techData.curStudiedTechNodeName != null && techData.curStudiedTechNodeName != "";
    }

    /// <summary>
    /// 判断一个科技节点是否正在被研究
    /// </summary>
    /// <param name="techNode"></param>
    /// <returns></returns>
    public bool IsTechNodeBeingStudied(ScriptableTechnologyNode techNode)
    {
        return techData.curStudiedTechNodeName == techNode.techName;
    }
}