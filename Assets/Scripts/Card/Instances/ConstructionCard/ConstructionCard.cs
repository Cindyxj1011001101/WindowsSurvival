using System.Collections.Generic;

public abstract class ConstructionCard : Card
{
    private ConstructionComponent construction;
    public override void Awake()
    {
        base.Awake();
        TryGetComponent(out construction);
        if (construction.canBeDemolished)
        {
            Events.Add(new("暴力拆毁", $"拆毁后获得{construction.demolitionDebris}", Event_DemolishThis, Judge_DemolishThis, () => 15));
        }
    }

    /// <summary>
    /// 拆毁建筑物
    /// </summary>
    private void DemolishThis(Card tool)
    {
        if (construction == null || !construction.canBeDemolished || string.IsNullOrEmpty(construction.demolitionDebris)) return;

        // 拆毁建筑物
        DestroyThis();
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("摧毁_01", true);
        // 消耗钢锤耐久
        tool.Use();

        // 消耗15分钟
        TimeManager.Instance.AddTime(15);

        // 掉落拆毁产物
        AddCards(ParseDemolitionDebris(construction.demolitionDebris), false);
    }

    private void Event_DemolishThis(out string tip)
    {
        tip = string.Empty;
        DemolishThis(GameManager.Instance.PlayerBag.FindCardOfName("钢锤"));
    }

    private bool Judge_DemolishThis(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("钢锤") == null)
        {
            hint = "需要钢锤";
            return false;
        }
        return true;
    }

    /// <summary>
    /// 解析拆毁产物
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private List<Card> ParseDemolitionDebris(string s)
    {
        List<Card> result = new();

        // 格式为：卡牌ID * 数量 + 卡牌ID * 数量 + ...
        var strs = s.Replace(" ", "").Split('+');
        string[] config;
        foreach (var str in strs)
        {
            config = str.Split('*');
            var card = CardFactory.CreateCard(config[0]);
            for (int i = 0; i < int.Parse(config[1]); i++)
            {
                result.Add(card);
            }
        }

        return result;
    }

    public override bool CanQuickInteract(Card card)
    {
        if (construction.canBeDemolished && card.CardId == "钢锤")
        {
            return true;
        }
        return base.CanQuickInteract(card);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        var card = slot.PeekCard();
        if (construction.canBeDemolished && card.CardId == "钢锤")
        {
            DemolishThis(card);
            return;
        }
        base.QuickIneract(slot, count, out tip);
    }
}