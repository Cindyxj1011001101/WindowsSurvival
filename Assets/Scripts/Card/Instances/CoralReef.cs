using System.Collections.Generic;

public class CoralReef : Card
{
    private RandomDropList dropList = new(
       new Drop(30, ("珊瑚", 1)),
       new Drop(8, ("海爬虫", 1)),
       new Drop(5, ("白爆矿", 1)),
       new Drop(2, ("有产物的水瓶鱼", 1))
       );

    private CoralReef()
    {
        Events = new()
        {
            new Event("用铲子凿", "用铲子凿珊瑚礁", Event_Dig, Judge_Dig, () => 45),
            new Event("欣赏", "一天内多次欣赏获得的数值会衰减", Event_Enjoy, null,() => 15,
            () => new Dictionary<PlayerStateEnum, float>() { { PlayerStateEnum.San, 6 * GlobalDataManager.Instance.saveData.GetReduceRate(CardId) }, { PlayerStateEnum.Sobriety, 4 * GlobalDataManager.Instance.saveData.GetReduceRate(CardId)} })
        };
    }

    protected override void Start()
    {
        GlobalDataManager.Instance.saveData.AddReduceAction(CardId, new Reduce(2));

        EventManager.Instance.AddListener(EventType.AnotherDay, RefreshSlot); // 隔天刷新
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.AnotherDay, RefreshSlot);
    }

    private void Event_Dig(out string tip)
    {
        DigByTool(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig), out tip);
    }

    private bool Judge_Dig(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig) == null)
        {
            hint = "需要挖掘类工具";
            return false;
        }
        return true;
    }

    private void Event_Enjoy(out string tip)
    {
        tip = string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, 6 * GlobalDataManager.Instance.saveData.GetReduceRate(CardId));
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, 4 * GlobalDataManager.Instance.saveData.GetReduceRate(CardId));

        GlobalDataManager.Instance.saveData.AddReduceCount(CardId);

        TimeManager.Instance.AddTime(15);
    }

    private void DigByTool(Card tool, out string tip)
    {
        RandomDrop(dropList, out tip, 2, beforeDrop: () =>
        {
            tool.Use();

            TimeManager.Instance.AddTime(45);

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound("挖掘废料_01", true);

        });
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        // 允许和带有挖掘标签的卡牌快速交互
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Dig))
        {
            tip = "用铲子凿";
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        DigByTool(slot.PeekCard(), out tip);
    }
}