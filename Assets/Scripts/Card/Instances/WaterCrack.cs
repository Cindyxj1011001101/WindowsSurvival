/// <summary>
/// 渗水裂缝
/// </summary>
public class WaterCrack : Card
{
    public override bool HasLoopSound => true;
    private WaterCrack()
    {
        Events = new()
        {
            new Event("堵住", "消耗裂缝填充物修补裂缝", Event_Fix, Jugde_Fix, () => 15),
        };
    }

    public void Event_Fix(out string tip)
    {
        StopUpdating();

        tip = string.Empty;
        SoundManager.Instance.PlaySound("堵住裂缝");
        EventManager.Instance.TriggerEvent(EventType.DialogueCondition,new SubscribeActionArgs("渗水裂缝","堵住"));
        TimeManager.Instance.AddTime(15);

        GameManager.Instance.PlayerBag.FindCardOfName("裂缝填充物").DestroyThis();
        DestroyThis();
    }

    public bool Jugde_Fix(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("裂缝填充物") == null)
        {
            hint = "需要裂缝填充物";
            return false;
        }
        return true;
    }

    protected override System.Action OnUpdate => () =>
    {
        var bag = Slot.Bag as EnvironmentBag;
        // 渗水裂缝所在的地点每回合-3氧气
        bag.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, -3);
        // 每个渗水裂缝每回合会使飞船水平面高度+0.3
        StateManager.Instance.ChangeWaterLevel(+0.3f);
    };
    public override void OnEnterEnvironment()
    {
        SoundManager.Instance.PlayCardLoopSound(CardId, "渗水声", 0.3f);
    }
    public override void OnLeaveEnvironment()
    {
        SoundManager.Instance.StopCardLoopSound(CardId);
    }
    public override void OnDetailOpen()
    {
        SoundManager.Instance.SetCardLoopVolume(CardId, 1.0f); // 音量调高
    }
    public override void OnDetailClose()
    {
        SoundManager.Instance.SetCardLoopVolume(CardId, 0.3f); // 恢复正常
    }
    public override void DestroyThis()
    {
        OnLeaveEnvironment();
        base.DestroyThis();
    }
}