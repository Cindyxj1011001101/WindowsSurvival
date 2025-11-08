using UnityEngine;

/// <summary>
/// 水瓶鱼
/// </summary>
public class AquariusFishWithProduct : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("用捕网捉", "肯定能捉到", Event_CatchByNet, Judge_CatchByNet, () => 15);
        AddCardEvent("用手捉", "可能捉不到", Event_CatchByHand, null, () => 30);
    }

    #region 用捕网捉
    private void Catch(Card tool, CardEvent e)
    {
        // 销毁卡牌
        DestroyThis();
        // 1. 消耗耐久
        tool.Use();

        ApplyEventEffects(e);

        // 3. 掉落卡牌
        // 获得一张“有产物的被捉住的水瓶鱼”
        AddCard("有产物的被捉住的水瓶鱼", GameManager.Instance.PlayerBag);
    }

    private void Event_CatchByNet(out string tip, CardEvent e)
    {
        tip = string.Empty;
        Catch(GameManager.Instance.PlayerBag.FindCardOfName("捞网"), e);
    }

    private bool Judge_CatchByNet(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("捞网") == null)
        {
            hint = "需要捞网";
            return false;
        }
        return true;
    }
    #endregion

    #region 用手捉
    private void Event_CatchByHand(out string tip, CardEvent e)
    {
        tip = string.Empty;
        DestroyThis();

        ApplyEventEffects(e);

        // 3/4概率逃跑
        var value = Random.value;
        if (value < 3 / 4f)
        {
            tip = "水瓶鱼逃跑了";
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sanity, -2);
            return;
        }

        // 获得一张“被捉住的水瓶鱼”
        // 继承产物进度
        // 添加到玩家背包
        AddCard("有产物的被捉住的水瓶鱼", GameManager.Instance.PlayerBag, out var card);
        card.InheritComponent<ProgressComponent>(this, out _);
    }
    #endregion

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        if (card.CardId == "捞网")
        {
            tip = Events[0].Name;
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        Catch(slot.PeekCard(), Events[0]);
    }
}