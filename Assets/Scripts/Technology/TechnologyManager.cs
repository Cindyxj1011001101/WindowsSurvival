using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TechnologyManager : MonoBehaviour
{
    private TechnologyManager instance = new();
    public TechnologyManager Instance => instance;

    private float basicStudyRate = 2.0f; // 基础研究速率，即每15分钟增长多少科技点

    private List<ScriptableTechnologyNode> studiedTechNodes = new(); // 学习过的科技节点

    private ScriptableTechnologyNode curStudiedTechNode; // 当前正在学习的科技节点
    private float curProgress; // 当前学习进度

    private TechnologyManager() { }

    /// <summary>
    /// 研究一个科技节点
    /// </summary>
    /// <param name="techNode"></param>
    public void Study(ScriptableTechnologyNode techNode)
    {
        curStudiedTechNode = techNode;
        curProgress = 0;
        // 添加监听，每回合结算研究进度
        EventManager.Instance.AddListener(EventType.IntervalSettle, OnStudy);
    }

    private void OnStudy()
    {
        // 计算研究速率
        float studyRate = CalcStudyRate();
        // 进度增长
        curProgress += studyRate;
        // 研究完成
        if (curProgress >= curStudiedTechNode.cost)
        {
            // 设置正在研究的科技节点为空
            curStudiedTechNode = null;
            // 设置研究进度为0
            curProgress = 0;
            // 解锁该科技
            UnlockTechNode(curStudiedTechNode);
            // 移除监听
            EventManager.Instance.RemoveListener(EventType.IntervalSettle, OnStudy);
        }
    }

    private float CalcStudyRate()
    {
        // 疲劳状态下有30%的研究速度减益，极度疲劳状态下有70%的研究速度减益。睡眠时不会进行研究。
        // 疲劳状态下有30%的研究速度减益，极度疲劳状态下有70%的研究速度减益。睡眠时不会进行研究。
        // 疲劳状态下有30%的研究速度减益，极度疲劳状态下有70%的研究速度减益。睡眠时不会进行研究。



        return basicStudyRate;
    }

    /// <summary>
    /// 解锁一个科技
    /// </summary>
    private void UnlockTechNode(ScriptableTechnologyNode techNode)
    {
        // 不要重复解锁
        if (studiedTechNodes.Contains(techNode)) return;

        // 将科技节点添加到已解锁列表中
        studiedTechNodes.Add(techNode);

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
        //return techNode.prerequisites.Count == 0 || techNode.prerequisites.All(studiedTechNodes.Contains);

        return false;
    }

    /// <summary>
    /// 判断一个科技节点能否研究
    /// </summary>
    /// <param name="techNode"></param>
    /// <returns></returns>
    public bool CanStudy(ScriptableTechnologyNode techNode)
    {
        // 如果科技未解锁，无法研究
        if (!IsTechNodeLocked(techNode)) return false;

        // 如果当前正在研究一个科技，无法研究
        return curStudiedTechNode == null;
    }
}