using UnityEngine;

/// <summary>
/// 白爆矿
/// </summary>
public class WhiteBlastMineStack : Card
{
    private WhiteBlastMineStack()
    {
        Events = new()
        {
            new Event("用铲子凿", "用铲子凿白爆矿堆",Event_Dig, Judge_Dig, () => 30)
        };
    }

    public void Event_Dig(out string tip)
    {
        tip = string.Empty;
        TimeManager.Instance.AddTime(30);
        //掉落卡牌
        RandomDrop();
        RandomDrop();

        GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig).TryUse();
        TryUse();
    }
    public bool Judge_Dig(out string hint)
    {
        hint = string.Empty;
        if(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig) == null)
        {
            hint = "需要挖掘类工具";
            return false;
        }
        return true;
    }
    public void RandomDrop()
    {
        int rand = Random.Range(0,16);
        if (rand < 4)
        {
            AddCards("白爆矿", 2, true);
        }
        else if (rand < 12)
        {
            AddCard("白爆矿", true);
        }
        else
        {
            AddCard("玻璃沙", true);
        }
    }
}