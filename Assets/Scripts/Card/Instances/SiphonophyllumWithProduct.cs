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
            new Event("切割", "这会杀死虹吸海葵并获得磁性触手", Event_Cut, Judge_Cut,() =>45),
            new Event("采集", "虹吸海葵上似乎富集了很多金属", Event_Collect, null,() => 15)
        };
    }

    public void Event_Cut(out string tip)
    {
        StopUpdating();

        tip = string.Empty;
        TimeManager.Instance.AddTime(45);
        AddCards("磁性触手", 3, true);

        GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut).TryUse();
        DestroyThis();
    }

    public bool Judge_Cut(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) == null)
        {
            hint = "需要切割类工具";
            return false;
        }
        return true;
    }

    public void Event_Collect(out string tip)
    {
        StopUpdating();

        tip = string.Empty;
        var sourceBag = Slot.Bag;
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

        DestroyThis();
    }
}