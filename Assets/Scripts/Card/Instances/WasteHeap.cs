using UnityEngine;

/// <summary>
/// 被安全泡沫覆盖的废料堆
/// </summary>
public class WasteHeap : Card
{
    public WasteHeap()
    {
        cardName = "废料堆";
        cardDesc = "被安全泡沫覆盖的废料堆。";
        cardType = CardType.ResourcePoint;
        maxStackNum = 1;
        moveable = false;
        weight = 0f;
        events = new()
        {
            new Event("挖掘", "挖掘废料堆", Event_Dig, null)
        };
        components = new()
        {
            { typeof(DurabilityComponent), new DurabilityComponent(5, OnDurabilityChanged) }
        };
    }

    private void OnDurabilityChanged(int durability)
    {
        if (durability == 0)
        {
            DestroyThis();
            slot.RefreshCurrentDisplay();
        }
    }

    public void Event_Dig()
    {
        //消耗1点耐久度
        TryGetComponent<DurabilityComponent>(out var component);
        component.Use();
        //消耗45分钟
        TimeManager.Instance.AddTime(45);
        //掉落卡牌
        RandomDrop();

    }

    public void RandomDrop()
    {
        int rand = Random.Range(0, 20);
        if (rand < 5)
        {
            GameManager.Instance.AddCard(new ScrapMetal(), true);
            GameManager.Instance.AddCard(new ScrapMetal(), true);
        }
        else if (rand < 9)
        {
            GameManager.Instance.AddCard(new ScrapMetal(), true);
        }
        else if (rand < 13)
        {
            GameManager.Instance.AddCard(new HardFiber(), true);
        }
        else if (rand < 16)
        {
            GameManager.Instance.AddCard(new CompactBiscuit(), true);
        }
        else if (rand < 17)
        {
            GameManager.Instance.AddCard(new RatBody(), true);
        }
        else if (rand < 18)
        {
            GameManager.Instance.AddCard(new RotMaterial(), true);
        }
        else
        {
            GameManager.Instance.AddCard(new OxygenCandle(), true);
        }
    }
}