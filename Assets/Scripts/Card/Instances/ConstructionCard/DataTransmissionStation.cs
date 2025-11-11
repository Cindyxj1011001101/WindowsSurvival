using System.Collections.Generic;

/// <summary>
/// 数据传输台
/// </summary>
public class DataTransmissionStation : ConstructionCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("数据传输", "使当前研究科技的研究进度+28\n（数据传输1天内最多可以进行2次）", Event_Transmit, Judge_Transmit,
            () => 60,
            () => new()
            {
                { PlayerStateEnum.Sobriety, -10 }
            },
            () => new()
            {
                { EnvironmentStateEnum.Electricity, -5f }
            });
        base.RegisterCardEvents(); // 拆毁
    }

    protected override void OnLateConstructor()
    {
        var states = new List<CardState>()
        {
            new ("待机中", "20", false, true, false),
            new ("运行中", "20", true, true, true),
        };
        stateMachine = new StateMachineComponent("待机中", states);
        AddComponent(stateMachine);
    }

    protected override void OnInit()
    {
        // 添加数据传输台使用次数的记录
        GlobalDataManager.Instance.GlobalData.AddReduceAction(CardId, new Reduce(2, .5f));

        EventManager.Instance.AddListener(EventType.AnotherDay, RefreshSlot); // 隔天时刷新
        EventManager.Instance.AddListener<ScriptableTechnologyNode>(EventType.StudyStarted, StartWorking);
        EventManager.Instance.AddListener(EventType.StudyStopped, StopWorking);

        // 当前有科技在研究
        if (TechnologyManager.Instance.CurStudiedTechNode != null)
        {
            StartWorking(TechnologyManager.Instance.CurStudiedTechNode);
        }
    }

    protected override void OnDestroy()
    {
        StopWorking();

        EventManager.Instance.RemoveListener(EventType.AnotherDay, RefreshSlot);
        EventManager.Instance.RemoveListener<ScriptableTechnologyNode>(EventType.StudyStarted, StartWorking);
        EventManager.Instance.RemoveListener(EventType.StudyStopped, StopWorking);
    }

    private void StartWorking(ScriptableTechnologyNode techNode)
    {
        if (techNode.techLevel != TechLevl.Intermediate) return;

        if (stateMachine.currentStateName == "运行中") return;

        stateMachine.ChangeState("运行中");
    }

    private void StopWorking()
    {
        if (stateMachine.currentStateName == "待机中") return;

        stateMachine.ChangeState("待机中");
    }

    /// <summary>
    /// 数据传输
    /// </summary>
    /// <param name="tip"></param>
    private void Event_Transmit(out string tip, CardEvent e)
    {
        tip = string.Empty;
        TechnologyManager.Instance.AddStudyProcess(28); // 研究进度增加
        GlobalDataManager.Instance.GlobalData.AddReduceCount(CardId); // 使用次数增加
        ApplyEventEffects(e);
    }

    private bool Judge_Transmit(out string hint)
    {
        hint = string.Empty;

        if (GlobalDataManager.Instance.GlobalData.IsReduceCountMax(CardId))
        {
            hint = "当日内可以进行的数据传输次数已达上限";
            return false;
        }

        if (TechnologyManager.Instance.CurStudiedTechNode == null)
        {
            hint = "当前没有科技在研究中，无法进行数据传输";
            return false;
        }

        if (StateManager.Instance.Electricity.CurValue < 5f)
        {
            hint = "电力供应不足";
            return false;
        }

        return true;
    }
}