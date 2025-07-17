using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 生贝肉
/// </summary>
public class RawOysterMeat : Card
{
    public RawOysterMeat()
    {
        //初始化参数
        cardName = "生贝肉";
        cardDesc = "肌纤维极其发达的贝肉，咬完感觉有点塞牙。希望生吃不会感染寄生虫。";
        cardType = CardType.Food;
        maxStackNum = 1;
        moveable = true;
        weight = 0.3f;
        curEndurance = maxEndurance = 1;
        tags = new();
        events = new List<Event>
        {
            new Event("食用", "食用生贝肉", Event_Eat, null),
        };
        components = new()
        {
            { typeof(FreshnessComponent), new FreshnessComponent(1440, OnFreshnessChanged) }
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
        //+6饱食
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, 6));
        //-1.2健康
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -1.2f));
        //消耗5分钟
        TimeManager.Instance.AddTime(5);
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
        PlayerBag playerBag = GameManager.Instance.PlayerBag;
        foreach (var slot in playerBag.Slots)
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
        PlayerBag playerBag = GameManager.Instance.PlayerBag;
        foreach (var slot in playerBag.Slots)
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