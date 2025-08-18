using System.Collections.Generic;
using Random = UnityEngine.Random;

/// <summary>
/// 诱捕陷阱
/// </summary>
public class Trap : Card
{
    private InnerContentsComponent innerContents;
    public bool isWorking; // 是否已打开
    public string OutcomeCardID;
    private Trap()
    {
        isWorking = false;
        Events = new()
        {
            new Event("布置", "布置", Event_Arrange, Judge_Arrange),
            new Event("收获", "收获", Event_TakeOut, Judge_TakeOut),
        };

        // 仅在室内、非水域地点建造
        AddComponent(new ConstructionComponent()
        {
            onlyInDoor = true,
            onlyOutWater = true,
            needCable = true,
        });
    }

    private bool ContentFilter(Card c, out string s)
    {
        // TODO
        throw new System.NotImplementedException();
    }

    public override bool CanQuickInteract(Card card)
    {
        return innerContents.CanQuickInteract(card);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        innerContents.QuickIneract(slot, count, out tip);
    }

    public void Event_TakeOut(out string tip)
    {
        tip = string.Empty;
        if (OutcomeCardID != null)
        {
            AddCard(OutcomeCardID, true);
            OutcomeCardID = null;
            Use();
        }
    }
    public bool Judge_TakeOut(out string hint)
    {
        hint = string.Empty;
        return OutcomeCardID != null;
    }
    public void Event_Arrange(out string tip)
    {
        tip = string.Empty;
        TimeManager.Instance.AddTime(15);
        isWorking = true;

    }

    public bool Judge_Arrange(out string hint)
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

        isWorking = false;
        Use();
        innerContents.Clear();

        foreach (var card in dropCards)
        {
            if (card.CardId == "有产物的水瓶鱼")
            {
                OutcomeCardID = "有产物的被捉住的水瓶鱼";
                //WAIT:可能需要处理生长度等的继承
            }
            else
            {
                OutcomeCardID = card.CardId;
            }
        }
    };
}