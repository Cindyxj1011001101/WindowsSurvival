using UnityEngine;

public class CoralReef : Card
{
    public int maxReduceCount;
    public int curReduceCount;
    public float ReduceRate;
    private CoralReef()
    {
        maxReduceCount = 2;
        curReduceCount = 0;
        ReduceRate = 0.5f;
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
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -2 * Mathf.Pow(ReduceRate, curReduceCount));
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, -2 * Mathf.Pow(ReduceRate, curReduceCount));
        curReduceCount++;
        if (curReduceCount >= maxReduceCount) curReduceCount = maxReduceCount;

    }
    //TODO:处理每过一天的重置次数
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