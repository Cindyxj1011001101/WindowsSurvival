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

    private void Event_Fix(out string tip)
    {
        Fix(GameManager.Instance.PlayerBag.FindCardOfName("裂缝填充物"), out tip);
    }

    private bool Jugde_Fix(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("裂缝填充物") == null)
        {
            hint = "需要裂缝填充物";
            return false;
        }
        return true;
    }

    private void Fix(Card patch, out string tip)
    {
        DestroyThis();
        patch.DestroyThis();

        tip = string.Empty;
        SoundManager.Instance.PlaySound("堵住裂缝");
        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("渗水裂缝", "堵住"));
        TimeManager.Instance.AddTime(15);
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        if (card.CardId == "裂缝填充物")
        {
            tip = "堵住";
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        Fix(slot.PeekCard(), out tip);
    }

    public override void OnAdd(Bag bag)
    {
        // 渗水裂缝所在的地点每回合-3氧气
        (bag as EnvironmentBag).ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.Oxygen, -3);
        StateManager.Instance.ChangeWaterLevelChangeRate(+0.3f);
    }

    public override void OnRemove(Bag bag)
    {
        (bag as EnvironmentBag).ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.Oxygen, +3);
        StateManager.Instance.ChangeWaterLevelChangeRate(-0.3f);
    }

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
}