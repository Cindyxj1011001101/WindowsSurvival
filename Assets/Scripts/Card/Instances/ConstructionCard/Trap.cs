using Newtonsoft.Json;
using System.Collections.Generic;
using Random = UnityEngine.Random;

/// <summary>
/// 诱捕陷阱
/// </summary>
[CardId("诱捕陷阱")]
public class Trap : ConstructionCard
{
    public override string ExtraInfo
    {
        get
        {
            if (caught) return "已捉到";
            else return base.ExtraInfo;
        }
    }

    [JsonProperty] private bool caught; // 是否捕捉到生物

    protected override void RegisterCardEvents()
    {
        AddCardEvent("布置", "布置诱捕陷阱，对当前地点内的生物进行诱捕", Event_Arrange, Judge_Arrange, () => 15);
        base.RegisterCardEvents(); // 拆毁
    }

    protected override void OnLateConstructor()
    {
        // 每个内容物槽的最大堆叠数为1
        foreach (var slot in innerContents.bag.Slots)
        {
            slot.SetMaxStackNum(1);
        }
    }

    protected override void OnInit()
    {
        innerContents.onRemoveCard = (c) =>
        {
            if (caught && innerContents.bag.IsEmpty)
            {
                caught = false;
                // 恢复内容物的可放入
                innerContents.allowAdd = true;
                stateMachine.ChangeState("未布置");
                Use();
            }
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

    /// <summary>
    /// 布置
    /// </summary>
    /// <param name="tip"></param>
    private void Event_Arrange(CardEvent e)
    {
        // 内容物停止更新
        innerContents.FreezeUpdate();

        // 不可添加或移除内容物
        innerContents.allowAdd = innerContents.allowRemove = false;
        innerContents.notAllowRemoveReason = "陷阱已布置，不能移除诱饵";
        innerContents.notAllowAddReason = "陷阱已布置，不能添加诱饵";

        // 先走时间，再变成已布置
        ApplyEventEffects(e, () => stateMachine.ChangeState("已布置"));
    }

    private bool Judge_Arrange(out string hint)
    {
        hint = string.Empty;
        if (caught)
        {
            hint = "请先取出捕捉到的生物";
            return false;
        }
        return stateMachine.currentStateName == "未布置";
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (caught || stateMachine.currentStateName == "未布置" || Bag is not EnvironmentBag env || env.DeepExploreDropList.IsEmpty) return;

        float prob = innerContents.bag.IsFull ? 1 / 3f : 1 / 48f;

        // 这个回合不抽牌
        if (Random.value > prob) return;

        // 从所在环境的深度探索列表中抽牌
        List<Card> dropCards = env.DeepExploreDropList.RandomDropTrappable();

        if (dropCards.IsNullOrEmpty()) return; // 没抽中

        // 抽中
        caught = true;
        
        // 清空内容物中的诱饵
        innerContents.Clear();

        // 恢复内容物的可移除
        innerContents.allowRemove = true;
        innerContents.notAllowAddReason = "不能添加，请先取出捕捉到的生物";

        // 添加卡牌
        Card outcomeCard;
        foreach (var card in dropCards)
        {
            if (CardFactory.ContainsCard("被捉住的" + card.CardId))
                outcomeCard = CardFactory.CreateCard("被捉住的" + card.CardId);
            else
                outcomeCard = card;

            AddCard(outcomeCard, innerContents.bag, forceAdd: true);
        }

        ShowTip("捉到了好东西");
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        return innerContents.CanQuickInteract(card, out tip);
    }

    public override void QuickIneract(SlotCards slot, int count)
    {
        innerContents.QuickIneract(slot, count);
    }
}