using System.Collections.Generic;

/// <summary>
/// 老鼠尸体
/// </summary>
public class RatBody : CookableCard
{
    private RandomDropList dropList = new(
       new Drop(3, ("小块生肉", 1)),
       new Drop(1, (out string tip) => { tip = "肉被糟蹋了，什么都没得到"; })
       );

    private RatBody()
    {
        Events = new()
        {
            new Event("食用", "不做任何处理，连同皮毛和内脏一起吃下", Event_Eat, null, () => 30,
            () => new Dictionary<PlayerStateEnum, float>() { { PlayerStateEnum.Fullness, 18 }, { PlayerStateEnum.San, -20 }, { PlayerStateEnum.Health, -8 } }),
            new Event("用手剥", "用手撕扯老鼠，这会弄得脏兮兮的，而且有小概率什么都拿不到", Event_PeelByHand, null, () => 45,
            () => new Dictionary<PlayerStateEnum, float>() { { PlayerStateEnum.San, -3 }, { PlayerStateEnum.Health, -2 } }),
            new Event("用刀切割", "可以采集到小块生肉", Event_PeelByKnife, Judge_PeelByKnife, () => 15),

        };
    }

    #region 食用
    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        // 播放吃的音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01", true);
        //+16饱食
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 18);
        //-20精神值
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -20);
        //-8健康
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -8);
        //消耗30分钟
        TimeManager.Instance.AddTime(30);
    }
    #endregion

    #region 用手剥
    private void Event_PeelByHand(out string tip)
    {
        DestroyThis();

        //-3精神值
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -3);
        //-2健康
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -2);
        //消耗45分钟
        TimeManager.Instance.AddTime(45);
        //随机掉落卡牌
        RandomDrop(dropList, out tip);
    }
    #endregion

    #region 用刀切割
    private void Event_PeelByKnife(out string tip)
    {
        PeelByKnife(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut), out tip);
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

    private void PeelByKnife(Card knife, out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        knife.Use();

        //消耗15分钟
        TimeManager.Instance.AddTime(15);
        AddCard("小块生肉", true);
    }
    #endregion

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        // 允许和带有切割标签的卡牌快速交互
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Cut))
        {
            tip = "用刀切割";
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        PeelByKnife(slot.PeekCard(), out tip);
    }
}