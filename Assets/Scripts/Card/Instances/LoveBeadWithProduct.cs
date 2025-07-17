using System.Collections.Generic;
using UnityEngine;

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
        events = new List<Event>()
        {
            new Event("撬开", "撬开爱情贝", Event_OpenByTool, Judge_OpenByTool),
        };
        tags = new List<CardTag>();
        components = new()
        {
            { typeof(ProgressComponent), new ProgressComponent(3600, null) },
        };
    }

    public LoveBeadWithProduct(int progress) : this()
    {
        TryGetComponent<ProgressComponent>(out var component);
        component.progress = progress;
    }

    #region 事件
    public void Event_OpenByTool()
    {
        Card tool = FindTool();
        if (tool != null)
        {
            tool.Use();
            GameManager.Instance.AddCard(new LoveBeadWithProduct(0), true);
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
    }

    public bool Judge_OpenByTool()
    {
        Card tool = FindTool();
        if (tool != null)
        {
            return true;
        }
        return false;
    }

    public Card FindTool()
    {
        foreach (var slot in GameManager.Instance.PlayerBag.Slots)  
        {
            if (slot.Cards[0].TryGetComponent<ToolComponent>(out var component))
            {
                if (component.toolType == ToolType.Dig || component.toolType == ToolType.Cut)
                {
                    return slot.Cards[0];
                }
            }
        }
        return null;
    }

    #endregion
}