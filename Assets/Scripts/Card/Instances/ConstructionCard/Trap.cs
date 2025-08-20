using System.Collections.Generic;
using Random = UnityEngine.Random;

/// <summary>
/// 诱捕陷阱
/// </summary>
public class Trap : Card
{
    private InnerContentsComponent innerContents;
    public bool isWorking; // 是否已打开
    public string outcomeCardId;
    private Trap()
    {
        isWorking = false;
        Events = new()
        {
            new Event("布置", "布置陷阱", Event_Arrange, Judge_Arrange, () => 15),
            new Event("收获", "取出捕捉到的生物", Event_TakeOut, Judge_TakeOut),
        };
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

    public override bool CanQuickInteract(Card card)
    {
        return innerContents.CanQuickInteract(card);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        innerContents.QuickIneract(slot, count, out tip);
    }

    private void Event_TakeOut(out string tip)
    {
        tip = string.Empty;
        Use();
        AddCard(outcomeCardId, true);
        outcomeCardId = null;
        isWorking = false;
        EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, this);
    }
    private bool Judge_TakeOut(out string hint)
    {
        hint = string.Empty;
        return string.IsNullOrEmpty(outcomeCardId);
    }
    private void Event_Arrange(out string tip)
    {
        tip = string.Empty;
        TimeManager.Instance.AddTime(15);
        isWorking = true;
        EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, this);
    }

    private bool Judge_Arrange(out string hint)
    {
        hint = string.Empty;
        return !isWorking;
    }

    protected override System.Action OnUpdate => () =>
    {
        int probability = 48;

        if (innerContents.bag.IsFull)
            probability = 3;

        // 这个回合不抽牌
        if (Random.Range(0, probability) != 0) return;

        List<Card> dropCards = GameManager.Instance.CurEnvironmentBag.RepeatableDropList.RandomDropTrappable();

        if (dropCards.IsNullOrEmpty()) return; // 没抽中

        // 抽中，清空内容物中的诱饵
        innerContents.Clear();

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
    };
}