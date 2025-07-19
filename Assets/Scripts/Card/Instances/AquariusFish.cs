using UnityEngine;

/// <summary>
/// 水瓶鱼
/// </summary>
public class AquariusFish : Card
{
    private AquariusFish()
    {
        events = new()
        {
            new Event("用捕网捉", "用捕网捉水瓶鱼", Event_CatchByNet, Judge_CatchByNet),
            new Event("用手捉", "用手捉水瓶鱼", Event_CatchByHand, null),
        };
    }

    private void OnProgressFull()
    {
        DestroyThis();
        GameManager.Instance.AddCard("有产物的水瓶鱼", true);
    }

    #region 用捕网捉
    public void Event_CatchByNet()
    {
        // 1. 消耗耐久

        // “捞网”耐久-1
        var tool = GameManager.Instance.PlayerBag.FindCardOfName("捞网");
        tool.TryUse();
        // 销毁卡牌
        DestroyThis();

        // 2. 时间变化
        TimeManager.Instance.AddTime(30);

        // 3. 掉落卡牌

        // 获得一张“被捉住的水瓶鱼”
        // 继承产物进度
        // 添加到玩家背包
        GameManager.Instance.AddCard("被捉住的水瓶鱼", true).InheritComponent<ProgressComponent>(this);
    }

    public bool Judge_CatchByNet()
    {
        return GameManager.Instance.PlayerBag.FindCardOfName("捞网") != null;
    }
    #endregion

    #region 用手捉
    public void Event_CatchByHand()
    {
        // 1. 销毁卡牌
        DestroyThis();

        int rand = Random.Range(0, 4);
        if (rand < 3)
        {
            // 2. 玩家状态变化
            StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -2));

            // 3. 时间变化
            TimeManager.Instance.AddTime(30);

            // 4. 鱼逃跑了    
        }
        else
        {
            TimeManager.Instance.AddTime(30);

            // 获得一张“被捉住的水瓶鱼”
            // 继承产物进度
            // 添加到玩家背包
            GameManager.Instance.AddCard("被捉住的水瓶鱼", true).InheritComponent<ProgressComponent>(this);
        }
    }
    #endregion

    protected override System.Action OnUpdate => () =>
    {
        TryGetComponent<ProgressComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval, OnProgressFull);
    };
}