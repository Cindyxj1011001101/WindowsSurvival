using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
            new Event("用铲子凿", "用铲子凿珊瑚礁", Event_Dig, Judge_Dig,() =>45),
            new Event("欣赏", "一天内多次欣赏获得的数值会衰减", Event_Enjoy, null,() => 15,
            () => new Dictionary<PlayerStateEnum, float>() { { PlayerStateEnum.San, 6 }, { PlayerStateEnum.Sobriety, 4 } })
        };
    }
    public void Event_Dig(out string tip)
    {
        GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig).TryUse();

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("挖掘废料_01", true);
        tip = string.Empty;
        TimeManager.Instance.AddTime(45);
        RandomDropByHand();
        RandomDropByHand();

    }
    public bool Judge_Dig(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig) == null)
        {
            hint = "需要挖掘类工具";
            return false;
        }
        return true;
    }
    public void Event_Enjoy(out string tip) 
    {
        tip = string.Empty;
        Debug.Log("珊瑚礁"+curReduceCount);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, 6 * Mathf.Pow(ReduceRate, curReduceCount));
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, 4* Mathf.Pow(ReduceRate, curReduceCount));
        TimeManager.Instance.AddTime(15);
        curReduceCount++;
        if (curReduceCount >= maxReduceCount) curReduceCount = maxReduceCount;

    }
    protected override Action OnUpdate => () =>
    {
        if (TimeManager.Instance.AnotherDay()) curReduceCount = 0;  
    };
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