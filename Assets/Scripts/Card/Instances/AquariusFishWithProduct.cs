using UnityEngine;

/// <summary>
/// 水瓶鱼
/// </summary>
public class AquariusFishWithProduct : Card
{
    private AquariusFishWithProduct()
    {
        Events = new()
        {
            new Event("用捕网捉", "肯定能捉到", Event_CatchByNet, Judge_CatchByNet, null, 30),
            new Event("用手捉", "可能捉不到", Event_CatchByHand, null, null, 30),
        };
    }

    #region 用捕网捉
    public void Event_CatchByNet(out string tip)
    {
        tip = string.Empty;
        // 1. 消耗耐久

        // “捞网”耐久-1
        var tool = GameManager.Instance.PlayerBag.FindCardOfName("捞网");
        tool.TryUse();
        // 销毁卡牌
        DestroyThis();

        // 2. 时间变化
        TimeManager.Instance.AddTime(30);

        // 3. 掉落卡牌

        // 获得一张“有产物的被捉住的水瓶鱼”
        AddCard("有产物的被捉住的水瓶鱼", true);
    }

    public bool Judge_CatchByNet()
    {
        return GameManager.Instance.PlayerBag.FindCardOfName("捞网") != null;
    }
    #endregion

    #region 用手捉
    public void Event_CatchByHand(out string tip)
    {
        tip = string.Empty;
        // 1. 销毁卡牌
        DestroyThis();

        int rand = Random.Range(0, 4);
        if (rand < 3)
        {
            // 2. 玩家状态变化
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -2);

            // 3. 时间变化
            TimeManager.Instance.AddTime(30);

            // 4. 鱼逃跑了
            tip = "水瓶鱼逃跑了";
        }
        else
        {
            TimeManager.Instance.AddTime(30);

            // 获得一张“有产物的被捉住的水瓶鱼”
            AddCard("有产物的被捉住的水瓶鱼", true);
        }
    }
    #endregion
}