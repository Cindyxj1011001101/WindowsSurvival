using UnityEngine;

/// <summary>
/// 被安全泡沫覆盖的废料堆
/// </summary>
public class WasteHeap : Card
{
    private WasteHeap()
    {
        Events = new()
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
            AddCards("废金属", 2, true);
        }
        else if (rand < 9)
        {
            AddCard("废金属", true);
        }
        else if (rand < 13)
        {
            AddCard("韧性胶管", true);
        }
        else if (rand < 16)
        {
            AddCard("压缩饼干", true);
        }
        else if (rand < 17)
        {
            AddCard("老鼠尸体", true);
        }
        else if (rand < 18)
        {
            AddCard("腐烂物", true);
        }
        else
        {
            AddCard("氧烛", true);
        }
    }
}