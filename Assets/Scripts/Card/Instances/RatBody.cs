using UnityEngine;

/// <summary>
/// 老鼠尸体
/// </summary>
public class RatBody : Card
{
    public RatBody()
    {
        events = new()
        {
            new Event("食用", "食用老鼠尸体", Event_Eat, null),
            new Event("用手剥", "用手剥老鼠尸体", Event_PeelByHand, null),
            new Event("用刀切割", "用刀切割老鼠尸体", Event_PeelByKnife, Judge_PeelByKnife),
        };
    }

    private void OnRotton()
    {
        DestroyThis();
        GameManager.Instance.AddCard("腐烂物", true);
    }

    #region 食用
    public void Event_Eat()
    {
        //销毁老鼠尸体
        DestroyThis();
        // 播放吃的音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01", true);
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
        DestroyThis();
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
        DestroyThis();
        var card = GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut);
        card.TryUse();
        //消耗15分钟
        TimeManager.Instance.AddTime(15);
        GameManager.Instance.AddCard("小块生肉", true);
    }

    public bool Judge_PeelByKnife()
    {
        return GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) != null;
    }
    #endregion

    #region 随机掉落
    public void RandomDrop()
    {
        int rand = Random.Range(0, 4);
        if (rand < 3)
        {
            GameManager.Instance.AddCard("小块生肉", true);
        }
        else if (rand < 4)
        {
            // 什么也没有掉落
        }
    }
    #endregion

    protected override System.Action OnUpdate => () =>
    {
        TryGetComponent<FreshnessComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval, OnRotton);
    };
}