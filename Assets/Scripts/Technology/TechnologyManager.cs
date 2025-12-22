using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum TechNodeState
{
    Locked,
    BeingStudied,
    Complished,
    Queued,
    ToStudy,
}

public class TechnologyManager : IManager
{
    public static TechnologyManager Instance { get; } = new();

    private const float POWER_CONSUMPTION_RATE_WHEN_STUDYING_INTERMEDIATE_TECHNOLOGIES = 0.5f; // 研究中级科技时每回合的电力消耗
    public const float BASIC_STUDY_RATE = 2.0f;     // 基础研究速率
    private const int MAX_STUDY_QUEUE_COUNT = 5;    // 最大研究队列长度

    public bool IsStudying { get; private set; }
    public List<string> StudyQueue { get; private set; } = new();                                   // 待研究科技节点队列
    public Dictionary<string, StudyProgressData> StudyProgressDict { get; private set; } = new();   // 科技节点进度字典

    private Dictionary<string, ScriptableTechnologyNode> allTechNodes = new();                      // 所有科技节点
    public bool IsIntermediateTechLocked { get; private set; }
    private string intermediateTechLockedReason;

    public ScriptableTechnologyNode CurStudiedTechNode
    {
        get
        {
            if (!IsStudying || StudyQueue.IsNullOrEmpty())
                return null;
            return allTechNodes[StudyQueue[0]];
        }
    }

    public bool AllTechComplished => StudyProgressDict.Values.All(t => t.Complished);
    public bool IsStudyQueueFull => StudyQueue.Count >= MAX_STUDY_QUEUE_COUNT;
    public bool IsStudyQueueEmpty => StudyQueue.Count == 0;

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

        var techData = GameDataManager.Instance.TechnologyData;

        IsStudying = techData.isStudying;
        StudyQueue = techData.studyQueue;

        // 初始化存档
        if (techData.studyProgressDict.IsNullOrEmpty())
        {
            techData.studyProgressDict = new();
            foreach (var node in allTechNodes.Values)
            {
                techData.studyProgressDict.Add(node.techName, new StudyProgressData(node.name, node.cost));
            }
        }

        // 解锁一遍物品配方
        StudyProgressDict = techData.studyProgressDict;
        foreach (var progress in StudyProgressDict.Values)
        {
            if (!progress.Complished) continue;

            foreach (var recipe in allTechNodes[progress.techName].recipes)
            {
                CraftManager.Instance.UnlockRecipe(recipe.cardId);
            }
        }

        // 检查
        CheckIntermediateTechUnlockCondition();

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
        IsStudying = false;
        StudyQueue = new();
        allTechNodes = new();
        StudyProgressDict = new();
        UpdateManager.Instance.TechnologyUpdate.RemoveListener(OnStudy);
        EventManager.Instance.RemoveListener<(string, int)>(EventType.CardNumChange, OnCardNumChanged);
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnElectricPowerChange);
    }

    public TechNodeState GetTechNodeState(ScriptableTechnologyNode node)
    {
        return GetTechNodeState(node, out _, out _);
    }

    public TechNodeState GetTechNodeState(ScriptableTechnologyNode node, out string lockedReason)
    {
        return GetTechNodeState(node, out lockedReason, out _);
    }

    public TechNodeState GetTechNodeState(ScriptableTechnologyNode node, out int studyOrder)
    {
        return GetTechNodeState(node, out _, out studyOrder);
    }

    public TechNodeState GetTechNodeState(ScriptableTechnologyNode node, out string lockedReason, out int studyOrder)
    {
        studyOrder = GetStudyOrder(node);
        if (IsTechNodeLocked(node, out lockedReason))
            return TechNodeState.Locked;

        if (IsTechNodeComplished(node))
            return TechNodeState.Complished;

        if (IsTechNodeBeingStudied(node))
            return TechNodeState.BeingStudied;

        if (studyOrder >= 0)
            return TechNodeState.Queued;

        return TechNodeState.ToStudy;
    }

    public ScriptableTechnologyNode GetTechNodeByName(string techName)
    {
        if (allTechNodes.TryGetValue(techName, out var node))
            return node;
        return null;
    }

    /// <summary>
    /// 将指定节点加入研究队列
    /// </summary>
    public void AddToStudyQueue(ScriptableTechnologyNode node)
    {
        // 节点为空
        if (node == null) return;

        // 已存在相同节点
        if (StudyQueue.Contains(node.techName)) return;

        // 研究队列已满
        if (IsStudyQueueFull) return;

        StudyQueue.Add(node.techName);

        if (StudyQueue.Count == 1)
            // 如果队列中只有该节点，开始研究
            Study(node);

        EventManager.Instance.TriggerEvent(EventType.RefreshStudyWindow);
    }

    /// <summary>
    /// 从研究队列中移除指定节点
    /// </summary>
    public void RemoveFromStudyQueue(ScriptableTechnologyNode node, bool complished = false)
    {
        // 节点为空
        if (node == null) return;

        var index = GetStudyOrder(node);

        // 不存在指定节点
        if (index < 0) return;

        if (index == 0)
            // 如果移除的是当前正在研究的节点，停止研究
            StopStudy(complished);

        // 移除节点
        StudyQueue.RemoveAt(index);

        // 研究下一个节点（如果有的话）
        if (StudyQueue.Count > 0)
            Study(allTechNodes[StudyQueue[0]]);

        EventManager.Instance.TriggerEvent(EventType.RefreshStudyWindow);
    }

    public int GetStudyOrder(ScriptableTechnologyNode node)
    {
        return StudyQueue.IndexOf(node.techName);
    }

    /// <summary>
    /// 立刻开始研究指定科技
    /// </summary>
    public void StudyImmediately(ScriptableTechnologyNode node)
    {
        if (node == null) return;

        var current = CurStudiedTechNode; // 保存当前研究的节点

        if (current != null)
        {
            // 如果当前正在研究的就是该节点，直接返回
            if (current.techName == node.techName) return;

            // 否则停止当前研究
            StopStudy();
            // 将当前研究的节点添加到目标节点的顺位
            StudyQueue.Remove(current.techName);
            var order = GetStudyOrder(node);
            if (order < 0)
                StudyQueue.Add(current.techName);
            else
                StudyQueue.Insert(order, current.techName);
        }

        // 将目标节点移至研究队列首位
        StudyQueue.Remove(node.techName);
        StudyQueue.Insert(0, node.techName);

        // 研究目标节点
        Study(node);

        EventManager.Instance.TriggerEvent(EventType.RefreshStudyWindow);
    }

    /// <summary>
    /// 研究一个科技节点
    /// </summary>
    /// <param name="techNode"></param>
    private void Study(ScriptableTechnologyNode techNode)
    {
        IsStudying = true;

        // 如果是中级科技，消耗电力
        if (techNode.techLevel == TechLevl.Intermediate)
            ElectricPowerManager.Instance.ConnectPower(nameof(TechnologyManager), POWER_CONSUMPTION_RATE_WHEN_STUDYING_INTERMEDIATE_TECHNOLOGIES);

        EventManager.Instance.TriggerEvent(EventType.StartStudy, techNode);
        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("StartResearch", techNode.techName));
    }

    /// <summary>
    /// 停止研究
    /// </summary>
    public void StopStudy(bool complished = false)
    {
        if (CurStudiedTechNode == null) return;

        var current = CurStudiedTechNode;

        // 恢复中级科技研究的电力消耗
        if (CurStudiedTechNode.techLevel == TechLevl.Intermediate)
            ElectricPowerManager.Instance.DisconnectPower(nameof(TechnologyManager));

        IsStudying = false;

        if (complished)
        {
            // 触发研究完成事件
            EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("FinishResearch", current.techName));
            EventManager.Instance.TriggerEvent(EventType.ComplishStudy, current);
        }
        else
        {
            // 触发研究暂停事件
            EventManager.Instance.TriggerEvent(EventType.StopStudy);
        }
    }

    /// <summary>
    /// 每15分钟结算研究进度
    /// </summary>
    private void OnStudy()
    {
        if (CurStudiedTechNode == null) return;

        // 进度增长
        AddStudyProgress(BASIC_STUDY_RATE);
    }

    public void AddStudyProgress(float value)
    {
        // 进度增长
        var techNodeProgress = StudyProgressDict[CurStudiedTechNode.techName];
        techNodeProgress.AddProgress(value);

        // 研究完成
        if (techNodeProgress.Complished)
        {
            SoundManager.Instance.PlaySound("研究完成", true);
            // 解锁该科技
            UnlockTechNode(CurStudiedTechNode);
            // 从研究队列中移除该节点
            RemoveFromStudyQueue(CurStudiedTechNode, true);
            return;
        }

        EventManager.Instance.TriggerEvent(EventType.RefreshStudyWindow);
    }

    public float GetStudyProgress(ScriptableTechnologyNode techNode)
    {
        return StudyProgressDict[techNode.techName].progress;
    }

    /// <summary>
    /// 解锁一个科技
    /// </summary>
    private void UnlockTechNode(ScriptableTechnologyNode techNode)
    {
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
        EventManager.Instance.TriggerEvent(EventType.RefreshStudyWindow);
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
        if (!(techNode.prerequisites.Count == 0 || techNode.prerequisites.All(IsTechNodeComplished)))
        {
            reason = "前置科技待完成";
            return true;
        }

        // 中级科技
        if (techNode.techLevel == TechLevl.Intermediate && IsIntermediateTechLocked)
        {
            reason = intermediateTechLockedReason;
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
        return IsTechNodeComplished(techNode.techName);
    }

    public bool IsTechNodeComplished(string techName)
    {
        return StudyProgressDict[techName].Complished;
    }

    /// <summary>
    /// 判断一个科技节点是否正在被研究
    /// </summary>
    /// <param name="techNode"></param>
    /// <returns></returns>
    public bool IsTechNodeBeingStudied(ScriptableTechnologyNode techNode)
    {
        return IsTechNodeBeingStudied(techNode.techName);
    }

    public bool IsTechNodeBeingStudied(string techName)
    {
        return CurStudiedTechNode != null && CurStudiedTechNode.techName == techName;
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
        var preLocked = IsIntermediateTechLocked;
        // 当前是否锁定
        IsIntermediateTechLocked = GetIsIntermediateTechLocked(out intermediateTechLockedReason);

        if (preLocked == IsIntermediateTechLocked) return;

        // 解锁 -> 锁定
        if (IsIntermediateTechLocked)
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
        EventManager.Instance.TriggerEvent(EventType.RefreshStudyWindow);
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
            EventManager.Instance.TriggerEvent(EventType.InterruptStudy, intermediateTechLockedReason);
        }

        // 将研究队列中的所有中级科技移除
        for (int i = StudyQueue.Count - 1; i >= 0; i--)
        {
            var node = allTechNodes[StudyQueue[i]];
            if (node.techLevel == TechLevl.Intermediate)
            {
                StudyQueue.RemoveAt(i);
            }
        }

        if (StudyQueue.Count > 0)
            Study(allTechNodes[StudyQueue[0]]);

        EventManager.Instance.TriggerEvent(EventType.RefreshStudyWindow);
    }
    #endregion
}