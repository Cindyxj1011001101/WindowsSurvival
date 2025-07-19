using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 有产物的爱情贝
/// </summary>
public class LoveBeadWithProduct : Card
{
    public LoveBeadWithProduct()
    {
        cardName = "爱情贝";
        cardDesc = "形似扇贝的古怪异星生物。其强大的肌肉使其能像鱼一样扇动贝壳游动。繁殖期的雄贝会搜集各类珍宝藏在贝壳中以吸引雌贝。";
        cardType = CardType.Creature;
        maxStackNum = 5;
        moveable = true;
        weight = 1.5f;
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
        GameManager.Instance.AddCard(new LoveBead(), true);
        TimeManager.Instance.AddTime(15);
        //撬开概率
        int random = Random.Range(0, 15);
        if (random < 3)
        {
            GameManager.Instance.AddCard(new GlassSand(), true);
            GameManager.Instance.AddCard(new GlassSand(), true);
        }
        else if (random < 6)
        {
            GameManager.Instance.AddCard(new ScrapMetal(), true);
            GameManager.Instance.AddCard(new ScrapMetal(), true);
        }
        else if (random < 9)
        {
            GameManager.Instance.AddCard(new Coral(), true);
        }
        else if (random < 12)
        {
            GameManager.Instance.AddCard(new HardFiber(), true);
        }
        else if (random < 15)
        {
            GameManager.Instance.AddCard(new WhiteBlastMine(), true);
        }
    }

    public bool Judge_OpenByTool()
    {
        return GameManager.Instance.PlayerBag.FindCardOfToolTypes(new List<ToolType> { ToolType.Cut, ToolType.Dig }) != null;
    }
    #endregion
}