using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TechnologyManager : IManager
{
    public static TechnologyManager Instance { get; } = new();

    private const float POWER_CONSUMPTION_RATE_WHEN_STUDYING_INTERMEDIATE_TECHNOLOGIES = 0.5f;

    private TechnologyData techData;

    private Dictionary<string, ScriptableTechnologyNode> allTechNodes = new();
    
    public float CurStudyRate { get; private set; }

    public ScriptableTechnologyNode CurStudiedTechNode
    {
        get
        {
            if (string.IsNullOrEmpty(techData.curStudiedTechNodeName) || !allTechNodes.TryGetValue(techData.curStudiedTechNodeName, out var value))
                return null;
            return value;
        }
    }

    public bool AllTechnologiesStudied => techData.studiedTechNodes.Count == techData.techNodeProgressDict.Count;

    private TechnologyManager() { }

    public void Init()
    {
        if (allTechNodes.IsNullOrEmpty())
        {
            // 注册所有科技节点
            foreach (var node in Resources.LoadAll<ScriptableTechnologyNode>("ScriptableObject/Technology"))
            {
                allTechNodes.Add(node.techName, node);
            }
        }

        techData = GameDataManager.Instance.TechnologyData;

        // 初始化存档
        if (techData.techNodeProgressDict.IsNullOrEmpty())
        {
            techData.techNodeProgressDict = new();
            foreach (var node in allTechNodes.Values)
            {
                techData.techNodeProgressDict.Add(node.techName, new TechNodeData { name = node.techName, progress = 0 });
            }
            techData.basicStudyRate = 2.0f;
            techData.isIntermediateTechLocked = true;
            techData.intermediateTechLockedReason = "缺少\"数据传输台\"";
        }

        CurStudyRate = CalcStudyRate();

        // 添加监听，每回合结算研究进度
        UpdateManager.Instance.TechnologyUpdate.AddListener(OnStudy);

        // 监听数据传输台的数量变化
        EventManager.Instance.AddListener<(string, int)>(EventType.CardNumChange, OnCardNumChanged);

        // 监听电力变化
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnElectricPowerChange);
        // 断电时检查中级科技解锁情况
        ElectricPowerManager.Instance.RegisterPowerOnOffActions(nameof(TechnologyManager), null, CheckIntermediateTechUnlockCondition);
    }

    public void Reset()
    {
        techData = null;
        UpdateManager.Instance.TechnologyUpdate.RemoveListener(OnStudy);
        EventManager.Instance.RemoveListener<(string, int)>(EventType.CardNumChange, OnCardNumChanged);
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnElectricPowerChange);
    }

    /// <summary>
    /// 研究一个科技节点
    /// </summary>
    /// <param name="techNode"></param>
    public void Study(ScriptableTechnologyNode techNode)
    {
        techData.curStudiedTechNodeName = techNode.techName;

        // 如果是中级科技，消耗电力
        if (techNode.techLevel == TechLevl.Intermediate)
            ElectricPowerManager.Instance.ConnectPower(nameof(TechnologyManager), POWER_CONSUMPTION_RATE_WHEN_STUDYING_INTERMEDIATE_TECHNOLOGIES);

        EventManager.Instance.TriggerEvent(EventType.StudyStarted, techNode);
        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("StartResearch", techNode.techName));
    }

    /// <summary>
    /// 停止研究
    /// </summary>
    public void StopStudy()
    {
        if (CurStudiedTechNode == null) return;

        // 恢复中级科技研究的电力消耗
        if (CurStudiedTechNode.techLevel == TechLevl.Intermediate)
            ElectricPowerManager.Instance.DisconnectPower(nameof(TechnologyManager));

        // 设置正在研究的科技节点为空
        techData.curStudiedTechNodeName = "";

        // 触发研究完成事件
        EventManager.Instance.TriggerEvent(EventType.StudyStopped);
    }

    /// <summary>
    /// 每15分钟结算研究进度
    /// </summary>
    private void OnStudy()
    {
        if (CurStudiedTechNode == null) return;
        
        // 计算研究速率
        CurStudyRate = CalcStudyRate();
        // 进度增长
        AddStudyProcess(CurStudyRate);
    }

    public void AddStudyProcess(float value)
    {
        // 进度增长
        techData.CurStudiedTechNodeData.progress += value;

        // 研究完成
        if (techData.CurStudiedTechNodeData.progress >= CurStudiedTechNode.cost)
        {
            SoundManager.Instance.PlaySound("研究完成", true);
            techData.CurStudiedTechNodeData.progress = CurStudiedTechNode.cost;
            // 解锁该科技
            UnlockTechNode(CurStudiedTechNode);
            EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("FinishResearch", techData.CurStudiedTechNodeData.name));
            // 触发研究完成事件
            EventManager.Instance.TriggerEvent(EventType.StudyComplished, CurStudiedTechNode);
            // 停止研究
            StopStudy();
            return;
        }

        EventManager.Instance.TriggerEvent(EventType.ChangeStudyProgress);
    }

    private float CalcStudyRate()
    {
        return techData.basicStudyRate;
    }

    public float GetStudyProgress(ScriptableTechnologyNode techNode)
    {
        return techData.techNodeProgressDict[techNode.techName].progress;
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
            CraftManager.Instance.UnlockRecipe(recipe.cardId);
        }
    }

    /// <summary>
    /// 开发用：立即解锁所有科技（会同时解锁配方）
    /// </summary>
    public void UnlockAllTechnologies()
    {
        if (allTechNodes.IsNullOrEmpty()) return;

        foreach (var node in allTechNodes.Values)
        {
            UnlockTechNode(node);
        }

        // 触发界面/进度刷新
        EventManager.Instance.TriggerEvent(EventType.ChangeStudyProgress);
    }

    /// <summary>
    /// 判断一个科技节点是否锁定
    /// </summary>
    /// <param name="techNode"></param>
    /// <returns></returns>
    public bool IsTechNodeLocked(ScriptableTechnologyNode techNode, out string reason)
    {
        reason = string.Empty;

        // 前置条件不满足，未解锁
        if (!(techNode.prerequisites.Count == 0 || techNode.prerequisites.All(t => techData.studiedTechNodes.Contains(t.techName))))
        {
            reason = "前置科技未解锁";
            return true;
        }

        // 中级科技
        if (techNode.techLevel == TechLevl.Intermediate && techData.isIntermediateTechLocked)
        {
            reason = techData.intermediateTechLockedReason;
            return true;
        }

        return false;
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

    #region 中级科技
    /// <summary>
    /// 中级科技是否锁定
    /// </summary>
    /// <returns></returns>
    private bool GetIsIntermediateTechLocked(out string reason)
    {
        reason = string.Empty;
        if (GlobalDataManager.Instance.GetCardNum("数据传输台") < 1)
        {
            reason = "缺少\"数据传输台\"";
            return true;
        }

        if (!ElectricPowerManager.Instance.IsAlreadyConnected(nameof(TechnologyManager)) && // 没有接电
            !ElectricPowerManager.Instance.CanConnectPower(POWER_CONSUMPTION_RATE_WHEN_STUDYING_INTERMEDIATE_TECHNOLOGIES, out reason)) // 并且不能接电
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 数据传输台数量变化时
    /// </summary>
    /// <param name="args"></param>
    private void OnCardNumChanged((string cardId, int num) args)
    {
        // 当数据传输台的数量变化时
        if (args.cardId != "数据传输台") return;

        // 检查中级科技解锁情况
        CheckIntermediateTechUnlockCondition();
    }

    /// <summary>
    /// 电力变化时执行
    /// </summary>
    private void OnElectricPowerChange(RefreshEnvironmentStateArgs args)
    {
        if (args.stateEnum != EnvironmentStateEnum.Electricity) return;

        CheckIntermediateTechUnlockCondition();
    }

    /// <summary>
    /// 检查中级科技解锁情况
    /// </summary>
    private void CheckIntermediateTechUnlockCondition()
    {
        // 原来是否锁定
        var preLocked = techData.isIntermediateTechLocked;
        // 当前是否锁定
        techData.isIntermediateTechLocked = GetIsIntermediateTechLocked(out techData.intermediateTechLockedReason);

        if (preLocked == techData.isIntermediateTechLocked) return;

        // 解锁 -> 锁定
        if (techData.isIntermediateTechLocked)
            LockIntermediateTechnologies();
        // 锁定 -> 解锁
        else
            UnlockIntermediateTechnologies();
    }

    /// <summary>
    /// 解锁中级科技
    /// </summary>
    private void UnlockIntermediateTechnologies()
    {
        EventManager.Instance.TriggerEvent(EventType.LockUnlockIntermediateTechnologies);
    }

    /// <summary>
    /// 锁定中级科技
    /// </summary>
    private void LockIntermediateTechnologies()
    {
        // 如果正在研究中级科技
        if (CurStudiedTechNode != null && CurStudiedTechNode.techLevel == TechLevl.Intermediate)
        {
            // 暂停当前研究
            StopStudy();
            // 显示原因
            EventManager.Instance.TriggerEvent(EventType.StudyInterrupted, techData.intermediateTechLockedReason);
        }

        EventManager.Instance.TriggerEvent(EventType.LockUnlockIntermediateTechnologies);
    }
    #endregion
}