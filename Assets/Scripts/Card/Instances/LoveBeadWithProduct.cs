using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 有产物的爱情贝
/// </summary>
public class LoveBeadWithProduct : Card
{
    public LoveBeadWithProduct()
    {
        events = new()
        {
            new Event("撬开", "撬开爱情贝", Event_OpenByTool, Judge_OpenByTool),
        };
    }

    #region 事件
    public void Event_OpenByTool()
    {
        DestroyThis();
        Card tool = GameManager.Instance.PlayerBag.FindCardOfToolTypes(new List<ToolType> { ToolType.Cut, ToolType.Dig });
        tool.TryUse();

        // 变回爱情贝
        GameManager.Instance.AddCard("爱情贝", true);
        TimeManager.Instance.AddTime(15);
        List<Card> cards = new();
        //撬开概率
        int random = Random.Range(0, 15);
        if (random < 3)
        {
            GameManager.Instance.AddCard("玻璃沙", true);
            GameManager.Instance.AddCard("玻璃沙", true);
        }
        else if (random < 6)
        {
            GameManager.Instance.AddCard("废金属", true);
            GameManager.Instance.AddCard("废金属", true);
        }
        else if (random < 9)
        {
            GameManager.Instance.AddCard("珊瑚", true);
        }
        else if (random < 12)
        {
            GameManager.Instance.AddCard("硬质纤维", true);
        }
        else if (random < 15)
        {
            GameManager.Instance.AddCard("白爆矿", true);
        }
    }

    public bool Judge_OpenByTool()
    {
        return GameManager.Instance.PlayerBag.FindCardOfToolTypes(new List<ToolType> { ToolType.Cut, ToolType.Dig }) != null;
    }
    #endregion
}