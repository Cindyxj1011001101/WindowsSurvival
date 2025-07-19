using UnityEngine;

/// <summary>
/// 有产物的虹吸海葵
/// </summary>
public class SiphonophyllumWithProduct : Card
{
    private SiphonophyllumWithProduct()
    {
        events = new()
        {
            new Event("切割", "切割虹吸海葵", Event_Cut, Judge_Cut),
            new Event("采集", "采集虹吸海葵", Event_Collect, null)
        };
    }

    public void Event_Cut()
    {
        DestroyThis();
        var card = GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut);
        card.TryUse();
        TimeManager.Instance.AddTime(45);
        GameManager.Instance.AddCard("磁性触手", true);
        GameManager.Instance.AddCard("磁性触手", true);
        GameManager.Instance.AddCard("磁性触手", true);
    }

    public bool Judge_Cut()
    {
        return GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) != null;
    }

    public void Event_Collect()
    {
        DestroyThis();
        // 变回虹吸海葵
        GameManager.Instance.AddCard("虹吸海葵", true);
        TimeManager.Instance.AddTime(15);
        int random = Random.Range(0, 6);
        if (random < 3)
        {
            GameManager.Instance.AddCard("废金属", true);
            GameManager.Instance.AddCard("废金属", true);
        }
        else if (random < 5)
        {
            GameManager.Instance.AddCard("废金属", true);
        }
        else
        {
            GameManager.Instance.AddCard("磁性触手", true);
        }
    }
}