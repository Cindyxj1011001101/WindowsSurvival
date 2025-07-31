using UnityEngine;

public class SeaGrassBed : Card
{
    private SeaGrassBed()
    {
        Events = new()
        {
            new Event("用手采集", "用手采集海麻线丛", Event_CollectByHand, null),
            new Event("用刀切割", "用刀切割海麻线丛", Event_CollectByKnife, Judge_CollectByKnife)
        };
    }

    public void Event_CollectByHand()
    {
        DestroyThis();
        TryUse();
        TimeManager.Instance.AddTime(30);
        RandomDropByHand();
        RandomDropByHand();
    }

    public bool Judge_CollectByKnife()
    {
        return GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) != null;
    }

    public void Event_CollectByKnife()
    {
        DestroyThis();
        var card = GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut);
        card.TryUse();
        RandomDropByKnife();
        RandomDropByKnife();
        RandomDropByKnife();
    }

    public void RandomDropByHand()
    {
        int rand = Random.Range(0, 22);
        if (rand < 4)
        {
            AddCard("海麻线", true);
            AddCard("海麻线", true);
        }
        else if (rand < 16)
        {
            AddCard("海麻线", true);
        }
        else if (rand < 19)
        {
            AddCard("海爬虫", true);
        }
        else if (rand < 21)
        {
            AddCard("海麻线根", true);
        }
        else
        {
            //掉落提示："手被划伤了"
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.PainLevel, 5);
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -3);
        }
    }
    public void RandomDropByKnife()
    {
        int rand = Random.Range(0, 20);
        if (rand < 10)
        {
            AddCard("海麻线", true);
            AddCard("海麻线", true);
        }
        else if (rand < 15)
        {
            AddCard("海麻线", true);
        }
        else if (rand < 18)
        {
            AddCard("海爬虫", true);
        }
        else
        {
            AddCard("海麻线根", true);
        }
    }
}