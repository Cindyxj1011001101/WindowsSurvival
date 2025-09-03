using System.Linq;
using UnityEngine;

public class TechnologyManager
{
    private static TechnologyManager instance = new();
    public static TechnologyManager Instance => instance;

    private TechnologyData techData;

    public ScriptableTechnologyNode CurStudiedTechNode => Resources.Load<ScriptableTechnologyNode>($"ScriptableObject/Technology/{techData.curStudiedTechNodeType}/{techData.curStudiedTechNodeName}");
    public float CurStudyRate { get; private set; }
    public bool AllTechnologiesStudied => techData.studiedTechNodes.Count == techData.techNodeDict.Count;

    private TechnologyManager()
    {
        techData = GameDataManager.Instance.TechnologyData;
    }

    public void InitFromGameData()
    {
        techData = GameDataManager.Instance.TechnologyData;
        if (CurStudiedTechNode != null)
            Study(CurStudiedTechNode);
        CurStudyRate = CalcStudyRate();
    }

    /// <summary>
    /// 研究一个科技节点
    /// </summary>
    /// <param name="techNode"></param>
    public void Study(ScriptableTechnologyNode techNode)
    {
        techData.curStudiedTechNodeName = techNode.techName;
        techData.curStudiedTechNodeType = techNode.techType;
        CurStudyRate = CalcStudyRate();
        // 添加监听，每回合结算研究进度
        UpdateManager.Instance.TechnologyUpdate.AddListener(OnStudy);
        EventManager.Instance.TriggerEvent(EventType.StudyStarted, CurStudiedTechNode);
        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("StartResearch", techNode.techName));
    }

    /// <summary>
    /// 停止研究
    /// </summary>
    public void StopStudy()
    {
        // 设置正在研究的科技节点为空
        techData.curStudiedTechNodeName = "";
        // 移除监听
        UpdateManager.Instance.TechnologyUpdate.RemoveListener(OnStudy);
        EventManager.Instance.TriggerEvent(EventType.StudyStoped);
    }

    /// <summary>
    /// 每15分钟结算研究进度
    /// </summary>
    private void OnStudy()
    {
        if (string.IsNullOrEmpty(techData.curStudiedTechNodeName)) return;
        
        // 计算研究速率
        CurStudyRate = CalcStudyRate();
        // 进度增长
        techData.CurStudiedTechNodeData.progress += CurStudyRate;
        // 研究完成
        if (techData.CurStudiedTechNodeData.progress >= CurStudiedTechNode.cost)
        {
            SoundManager.Instance.PlaySound("研究完成", true);
            techData.CurStudiedTechNodeData.progress = CurStudiedTechNode.cost;
            // 解锁该科技
            UnlockTechNode(CurStudiedTechNode);
            // 触发研究完成事件
            EventManager.Instance.TriggerEvent(EventType.StudyComplished, CurStudiedTechNode);
            EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("FinishResearch", CurStudiedTechNode.techName));
            // 停止研究
            StopStudy();
        }
        EventManager.Instance.TriggerEvent(EventType.ChangeStudyProgress);
    }

    public void AddStudyProcess(int value)
    {
        // 进度增长
        techData.CurStudiedTechNodeData.progress += CurStudyRate;
        // 研究完成
        if (techData.CurStudiedTechNodeData.progress >= CurStudiedTechNode.cost)
        {
            SoundManager.Instance.PlaySound("研究完成", true);
            techData.CurStudiedTechNodeData.progress = CurStudiedTechNode.cost;
            // 解锁该科技
            UnlockTechNode(CurStudiedTechNode);
            // 触发研究完成事件
            EventManager.Instance.TriggerEvent(EventType.StudyComplished, CurStudiedTechNode);
            // 停止研究
            StopStudy();
        }
        EventManager.Instance.TriggerEvent(EventType.ChangeStudyProgress);
    }

    private float CalcStudyRate()
    {
        return techData.basicStudyRate;
    }

    public float GetStudyProgress(ScriptableTechnologyNode techNode)
    {
        return techData.techNodeDict[techNode.techName].progress;
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
    public bool IsTechNodeComplished(ScriptableTechnologyNode techNode)
    {
        return techData.studiedTechNodes.Contains(techNode.techName);
    }

    public bool IsTechNodeComplished(string techName)
    {
        return techData.studiedTechNodes.Contains(techName);
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
    public bool IsTechNodeBeingStudied(string techName)
    {
        return techData.curStudiedTechNodeName == techName;
    }
}