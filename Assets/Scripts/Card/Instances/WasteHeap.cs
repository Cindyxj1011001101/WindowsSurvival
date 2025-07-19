using UnityEngine;

/// <summary>
/// 被安全泡沫覆盖的废料堆
/// </summary>
public class WasteHeap : Card
{
    public WasteHeap()
    {
        events = new()
        {
            new Event("挖掘", "挖掘废料堆", Event_Dig, null)
        };
    }

    public void Event_Dig()
    {
        //消耗1点耐久度
        TryUse();
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
            GameManager.Instance.AddCard("废金属", true);
            GameManager.Instance.AddCard("废金属", true);
        }
        else if (rand < 9)
        {
            GameManager.Instance.AddCard("废金属", true);
        }
        else if (rand < 13)
        {
            GameManager.Instance.AddCard("硬质纤维", true);
        }
        else if (rand < 16)
        {
            GameManager.Instance.AddCard("压缩饼干", true);
        }
        else if (rand < 17)
        {
            GameManager.Instance.AddCard("老鼠尸体", true);
        }
        else if (rand < 18)
        {
            GameManager.Instance.AddCard("腐烂物", true);
        }
        else
        {
            GameManager.Instance.AddCard("氧烛", true);
        }
    }
}