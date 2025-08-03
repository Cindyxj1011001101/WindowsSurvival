using UnityEngine;

public class CoralReef : Card
{
    private CoralReef()
    {
        Events = new()
        {
            new Event("用铲子凿", "用铲子凿珊瑚礁", Event_Dig, Judge_Dig),
            new Event("欣赏", "欣赏珊瑚礁", Event_Enjoy, null),
        };
    }
    public void Event_Dig(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        var card = GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig);
        card.TryUse();
        TimeManager.Instance.AddTime(45);
        RandomDropByHand();
        RandomDropByHand();
    }
    public bool Judge_Dig()
    {
        return GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig) != null;
    }
    public void Event_Enjoy(out string tip)
    {
        tip = string.Empty;
        TimeManager.Instance.AddTime(15);
        if (TryGetComponent<DailyReduceComponent>(out var component))
        {
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, component.CalReduce(4));
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, component.CalReduce(6));
            component.AddReduceCount();
        }
    }
    public void RandomDropByHand()
    {
        int rand = Random.Range(0, 45);
        if (rand < 30)
        {
            AddCard("珊瑚", true);
        }
        else if (rand < 38)
        {
            AddCard("海爬虫", true);
        }
        else if (rand < 43)
        {
            AddCard("白爆矿", true);
        }
        else
        {
            AddCard("有产物的水瓶鱼", false);
        }
    }
}