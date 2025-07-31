using UnityEngine;

/// <summary>
/// 有产物的虹吸海葵
/// </summary>
public class SiphonophyllumWithProduct : Card
{
    private SiphonophyllumWithProduct()
    {
        Events = new()
        {
            new Event("切割", "这会杀死虹吸海葵并获得磁性触手", Event_Cut, Judge_Cut, "需要切割工具",45),
            new Event("采集", "虹吸海葵上似乎富集了很多金属", Event_Collect, null,null, 15)
        };
    }

    public void Event_Cut()
    {
        DestroyThis();
        var card = GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut);
        card.TryUse();
        TimeManager.Instance.AddTime(45);
        AddCards("磁性触手", 3, true);
    }

    public bool Judge_Cut()
    {
        return GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) != null;
    }

    public void Event_Collect()
    {
        var sourceBag = Slot.Bag;
        DestroyThis();
        // 变回虹吸海葵
        AddCard("虹吸海葵", sourceBag is PlayerBag);
        TimeManager.Instance.AddTime(15);
        int random = Random.Range(0, 6);
        if (random < 3)
        {
            AddCards("废金属", 2, true);
        }
        else if (random < 5)
        {
            AddCard("废金属", true);
        }
        else
        {
            AddCard("磁性触手", true);
        }
    }
}