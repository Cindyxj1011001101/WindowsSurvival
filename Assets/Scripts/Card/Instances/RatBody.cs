using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 老鼠尸体
/// </summary>
public class RatBody : Card
{
    public RatBody()
    {
        //初始化参数
        cardName = "老鼠尸体";
        cardDesc = "一只老鼠的尸体，可以用来制作食物。";
        //cardImage = Resources.Load<Sprite>("CardImage/老鼠尸体");
        cardType = CardType.Food;
        maxStackNum = 1;
        moveable = true;
        weight = 0.7f;
        curEndurance = maxEndurance = 1;
        tags = new();
        events = new List<Event>
        {
            new Event("食用", "食用老鼠尸体", Event_Eat, null),
            new Event("用手剥", "用手剥老鼠尸体", Event_PeelByHand, null),
            new Event("用刀切割", "用刀切割老鼠尸体", Event_PeelByKnife, Judge_PeelByKnife),
        };
        components = new()
        {
            { typeof(FreshnessComponent), new FreshnessComponent(2880, OnFreshnessChanged) }
        };
    }

    private void OnFreshnessChanged(int freshness)
    {
        if (freshness == 0)
        {
            Use();
            GameManager.Instance.AddCard(new RotMaterial(), true);
        }
    }

    #region 食用
    public void Event_Eat()
    {
        //销毁老鼠尸体
        Use();
        //+16饱食
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, 16));
        //-20精神值
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -20));
        //-8健康
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -8));
        //消耗30分钟
        TimeManager.Instance.AddTime(30);
    }
    #endregion

    #region 用手剥
    public void Event_PeelByHand()
    {
        //销毁老鼠尸体
        Use();
        //-3精神值
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -3));
        //-2健康
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -2));
        //消耗45分钟
        TimeManager.Instance.AddTime(45);
        //随机掉落卡牌
        RandomDrop();
    }
    #endregion

    #region 用刀切割
    public void Event_PeelByKnife()
    {
        Use();
        foreach (var slot in GameManager.Instance.PlayerBag.Slots)
        {
            if (slot.PeekCard() != null && slot.PeekCard().TryGetComponent<ToolComponent>(out var component))
            {
                if (component.toolType == ToolType.Cut)
                {
                    slot.PeekCard().Use();
                    break;
                }
            }
        }
        //消耗15分钟
        TimeManager.Instance.AddTime(15);
        GameManager.Instance.AddCard(new LittleRawMeat(), true);
    }

    public bool Judge_PeelByKnife()
    {
        foreach (var slot in GameManager.Instance.PlayerBag.Slots)
        {
            if (slot.PeekCard() != null && slot.PeekCard().TryGetComponent<ToolComponent>(out var component))
            {
                if (component.toolType == ToolType.Cut)
                {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region 随机掉落
    public void RandomDrop()
    {
        int rand = Random.Range(0, 4);
        if (rand < 3)
        {
            GameManager.Instance.AddCard(new LittleRawMeat(), true);
        }
        else if (rand < 4)
        {
            //GameManager.Instance.AddCard(null, true);
        }
    }
    #endregion

    protected override System.Action OnUpdate => () =>
    {
        TryGetComponent<FreshnessComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval);
    };
}