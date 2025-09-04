using System.Collections.Generic;

/// <summary>
/// 数据传输台
/// </summary>
public class DataTransmissionStation : ConstructionCard
{
    private StateMachineComponent stateMachine;

    public float electricityConsume = 0.5f; // 每回合电力消耗

    private DataTransmissionStation()
    {
        Events = new()
        {
            new Event("数据传输", "使当前研究科技的研究进度加28" +
            $"\n（数据传输1天内最多可以进行2次，当日已进行次数:  {GlobalDataManager.Instance.saveData.GetCurrentReduceCount(CardId)}）",
            Event_Transmit,
            Judge_Transmit,
            () => 60,
            () => new() { { PlayerStateEnum.Sobriety, -10 } }, () => new() { { EnvironmentStateEnum.Electricity, -5f } }),
        };
    }

    public override void LateInit()
    {
        base.LateInit();

        EventManager.Instance.AddListener(EventType.AnotherDay, RefreshSlot); // 隔天时刷新
        EventManager.Instance.AddListener(EventType.StudyStarted, StartWorking);
        EventManager.Instance.AddListener(EventType.StudyStoped, StopWorking);

        // 添加数据传输台使用次数的记录
        GlobalDataManager.Instance.saveData.AddReduceAction(CardId, new Reduce(2));

        // 未布置和已布置两种状态
        if (!TryGetComponent(out stateMachine))
        {
            var states = new List<CardState>()
            {
                new ("待机中", "20", false, true, false),
                new ("运行中", "20", true, true, true),
            };
            stateMachine = new StateMachineComponent("待机中", states);
            AddComponent(stateMachine);
        }

        // 当前有科技在研究
        if (TechnologyManager.Instance.CurStudiedTechNode != null)
        {
            StartWorking();
        }
    }

    public override void DestroyThis()
    {
        base.DestroyThis();

        StopWorking();
        EventManager.Instance.RemoveListener(EventType.AnotherDay, RefreshSlot);
        EventManager.Instance.RemoveListener(EventType.StudyStarted, StartWorking);
        EventManager.Instance.RemoveListener(EventType.StudyStoped, StopWorking);
    }

    private void StartWorking()
    {
        if (stateMachine.currentStateName == "运行中") return;

        stateMachine.ChangeState("运行中");
        StateManager.Instance.ChangeElectricityChangeRate(-electricityConsume);
    }

    private void StopWorking()
    {
        if (stateMachine.currentStateName == "待机中") return;

        stateMachine.ChangeState("待机中");
        StateManager.Instance.ChangeElectricityChangeRate(+electricityConsume);
    }

    /// <summary>
    /// 数据传输
    /// </summary>
    /// <param name="tip"></param>
    private void Event_Transmit(out string tip)
    {
        tip = string.Empty;
        StateManager.Instance.ChangeElectricity(-5f);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, -10);

        TechnologyManager.Instance.AddStudyProcess(28); // 研究进度增加

        GlobalDataManager.Instance.saveData.AddReduceCount(CardId); // 使用次数增加

        TimeManager.Instance.AddTime(60);
    }

    private bool Judge_Transmit(out string hint)
    {
        hint = string.Empty;

        if (GlobalDataManager.Instance.saveData.IsReduceCountMax(CardId))
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
            hint = "当前电力过低，无法进行数据传输";
            return false;
        }

        return true;
    }
}