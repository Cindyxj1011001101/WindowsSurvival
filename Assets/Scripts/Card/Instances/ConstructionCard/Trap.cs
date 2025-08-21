using System.Collections.Generic;
using Random = UnityEngine.Random;

/// <summary>
/// 诱捕陷阱
/// </summary>
public class Trap : ConstructionCard
{
    private InnerContentsComponent innerContents;

    public bool isArranged = false; // 是否已布置
    public string outcomeCardId = null; // 诱捕产物

    private Trap()
    {
        Events = new()
        {
            new Event("布置", "", Event_Arrange, Judge_Arrange, () => 15),
            new Event("取出", "取出捕捉到的生物", Event_TakeOut, Judge_TakeOut),
        };
    }

    protected override void LateInit()
    {
        base.LateInit();

        // 每个卡牌槽的最大堆叠数都为1
        foreach (var slot in innerContents.bag.Slots)
        {
            slot.SetMaxStackNum(1);
        }
    }

    private bool ContentFilter(Card c, out string s)
    {
        s = string.Empty;
        if (c.CardType != CardType.Food)
        {
            s = "只能放入食物";
            return false;
        }
        return true;
    }

    /// <summary>
    /// 取出
    /// </summary>
    /// <param name="tip"></param>
    private void Event_TakeOut(out string tip)
    {
        tip = string.Empty;
        Use();
        AddCard(outcomeCardId, true);
        outcomeCardId = null;
    }

    private bool Judge_TakeOut(out string hint)
    {
        hint = string.Empty;
        return !string.IsNullOrEmpty(outcomeCardId);
    }

    /// <summary>
    /// 布置
    /// </summary>
    /// <param name="tip"></param>
    private void Event_Arrange(out string tip)
    {
        tip = string.Empty;

        // 内容物停止更新
        innerContents.PauseUpdating();

        // 不可添加或移除内容物
        innerContents.canAddOrRemove = false;

        TimeManager.Instance.AddTime(15);
        isArranged = true;
    }

    private bool Judge_Arrange(out string hint)
    {
        hint = string.Empty;
        if (!string.IsNullOrEmpty(outcomeCardId))
        {
            hint = "请先取出捕捉到的生物";
            return false;
        }
        return !isArranged;
    }

    protected override System.Action OnUpdate => () =>
    {
        if (!isArranged || Bag is not EnvironmentBag env || env.RepeatableDropList.IsEmpty) return;

        int probability = innerContents.bag.IsFull ? 3 : 48;

        // 这个回合不抽牌
        if (Random.Range(0, probability) != 0) return;

        // 从所在环境的深度探索列表中抽牌
        List<Card> dropCards = env.RepeatableDropList.RandomDropTrappable();

        if (dropCards.IsNullOrEmpty()) return; // 没抽中

        // 抽中
        foreach (var card in dropCards)
        {
            if (card.CardId == "有产物的水瓶鱼")
            {
                outcomeCardId = "有产物的被捉住的水瓶鱼";
                //WAIT:可能需要处理生长度等的继承
            }
            else
            {
                outcomeCardId = card.CardId;
            }
        }

        // 抽中，清空内容物中的诱饵
        innerContents.Clear();

        // 恢复内容物的可添加移除
        innerContents.canAddOrRemove = true;
        RefreshSlot();
        ShowTip("捉到了好东西");
    };

    public override bool CanQuickInteract(Card card)
    {
        return innerContents.CanQuickInteract(card);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        innerContents.QuickIneract(slot, count, out tip);
    }
}