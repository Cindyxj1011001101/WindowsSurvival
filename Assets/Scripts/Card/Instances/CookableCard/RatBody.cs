/// <summary>
/// 老鼠尸体
/// </summary>
public class RatBody : CookableCard
{
    private DropList dropList = new(
       new Drop(3, ("小块生肉", 1)),
       new Drop(1, (out string tip) => { tip = "肉被糟蹋了，什么都没得到"; })
       );

    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "不做任何处理，连同皮毛和内脏一起吃下", EasyEvent_Destroy, null,
            () => 30,
            () => new()
            {
                { PlayerStateEnum.Hunger, 18 },
                { PlayerStateEnum.Sanity, -20 },
                { PlayerStateEnum.Health, -8 }
            },
            sound: "吃_01");
        AddCardEvent("用手剥", "用手撕扯老鼠，这会弄得脏兮兮的，而且有小概率什么都拿不到", Event_PeelByHand, null,
            () => 45,
            () => new()
            {
                { PlayerStateEnum.Sanity, -3 },
                { PlayerStateEnum.Health, -2 }
            });
        AddCardEvent("用刀切割", "可以采集到小块生肉", Event_PeelByKnife, Judge_PeelByKnife, () => 15);
    }

    #region 用手剥
    private void Event_PeelByHand(out string tip, CardEvent e)
    {
        DestroyThis();
        ApplyEventEffects(e);
        //随机掉落卡牌
        RandomDrop(dropList, out tip);
    }
    #endregion

    #region 用刀切割
    private void Event_PeelByKnife(out string tip, CardEvent e)
    {
        tip = string.Empty;
        PeelByKnife(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut), e);
    }

    private bool Judge_PeelByKnife(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) == null)
        {
            hint = "需要切割类工具";
            return false;
        }
        return true;
    }

    private void PeelByKnife(Card knife, CardEvent e)
    {
        DestroyThis();
        knife.Use();
        ApplyEventEffects(e);
        AddCard("小块生肉", true);
    }
    #endregion

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        // 允许和带有切割标签的卡牌快速交互
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Cut))
        {
            tip = Events[2].Name;
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        PeelByKnife(slot.PeekCard(), Events[2]);
    }
}