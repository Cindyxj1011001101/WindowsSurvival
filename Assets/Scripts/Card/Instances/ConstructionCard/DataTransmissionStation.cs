using System.Collections.Generic;

/// <summary>
/// 数据传输台
/// </summary>
[CardId("数据传输台")]
public class DataTransmissionStation : ConstructionCard
{
    // 表示该卡牌具有循环音效
    // SoundManager 会根据玩家是否在同一环境、以及详情窗口打开状态来播放/调节该卡牌的循环音效
    public override bool HasLoopSound => true;
    private const int STUDY_PROGRESS_ADD = 28;

    protected override void RegisterCardEvents()
    {
        AddCardEvent("数据传输", $"使正在研究科的技的进度{ColorManager.Colorize(STUDY_PROGRESS_ADD, ColorManager.Green)}\n（数据传输1天内最多可以进行2次）", Event_Transmit, Judge_Transmit,
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
            new ("待机中", "20", false),
            new ("运行中", "20", true),
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
        // 开始运行时播放循环音（仅当玩家在同一地点时）
        // 这样可以让玩家听到持续的工作声，进入/退出地点或打开详情会由其他回调控制音量与停止
        if (GameManager.Instance.IsCurrentEnvironment(Bag))
            SoundManager.Instance.PlayCardLoopSound(CardId, "数据传输台循环音", 0.3f);
    }

    private void StopWorking()
    {
        if (stateMachine.currentStateName == "待机中") return;

        stateMachine.ChangeState("待机中");
        // 停止运行时停止循环音（仅当玩家在同一地点时）
        if (GameManager.Instance.IsCurrentEnvironment(Bag))
            SoundManager.Instance.StopCardLoopSound(CardId);
    }

    public override void OnEnterEnvironment()
    {
        // 玩家进入卡牌所在地点时调用：若当前处于运行中则播放循环音
        if (stateMachine != null && stateMachine.currentStateName == "运行中")
            SoundManager.Instance.PlayCardLoopSound(CardId, "数据传输台循环音", 0.3f);
    }
    public override void OnLeaveEnvironment()
    {
        // 玩家离开卡牌所在地点时调用：停止循环音
        SoundManager.Instance.StopCardLoopSound(CardId);
    }
    public override void OnDetailOpen()
    {
        // 打开卡牌详情界面时调用：将循环音音量调高以突出音效
        SoundManager.Instance.SetCardLoopVolume(CardId, 1.0f);
    }
    public override void OnDetailClose()
    {
        // 关闭卡牌详情界面时调用：将循环音恢复到默认的较低音量
        SoundManager.Instance.SetCardLoopVolume(CardId, 0.3f);
    }

    /// <summary>
    /// 数据传输
    /// </summary>
    /// <param name="tip"></param>
    private void Event_Transmit(CardEvent e)
    {
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

        if (ElectricPowerManager.Instance.Power.CurValue < 5f)
        {
            hint = "电力供应不足";
            return false;
        }

        return true;
    }
}