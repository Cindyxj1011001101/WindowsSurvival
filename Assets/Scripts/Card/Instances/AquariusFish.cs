using UnityEngine;

/// <summary>
/// 水瓶鱼
/// </summary>
public class AquariusFish : Card
{
    private AquariusFish()
    {
        Events = new()
        {
            new Event("用捕网捉", "肯定能捉到", Event_CatchByNet, Judge_CatchByNet, () => 15),
            new Event("用手捉", "可能捉不到", Event_CatchByHand, null, () => 30),
        };
    }

    private void OnProgressFull()
    {
        DestroyThis();
        AddCard("有产物的水瓶鱼", Bag);
    }

    #region 用捕网捉
    private void Event_CatchByNet(out string tip)
    {
        Catch(GameManager.Instance.PlayerBag.FindCardOfName("捞网"), out tip);
    }

    private bool Judge_CatchByNet(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("捞网") == null)
        {
            hint = "需要捞网";
            return false;
        }
        return true;
    }
    #endregion

    #region 用手捉
    private void Event_CatchByHand(out string tip)
    {
        DestroyThis();

        tip = string.Empty;

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

            // 获得一张“被捉住的水瓶鱼”
            // 继承产物进度
            // 添加到玩家背包
            AddCard("被捉住的水瓶鱼", true, out var card);
            card.InheritComponent<ProgressComponent>(this);
        }
    }
    #endregion

    protected override System.Action OnUpdate => () =>
    {
        TryGetComponent<ProgressComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval, OnProgressFull);
    };


    private void Catch(Card tool, out string tip)
    {
        // 销毁卡牌
        DestroyThis();
        // 1. 消耗耐久
        tool.Use();

        tip = string.Empty;

        // 2. 时间变化
        TimeManager.Instance.AddTime(15);

        // 3. 掉落卡牌
        // 获得一张“被捉住的水瓶鱼”
        AddCard("被捉住的水瓶鱼", true, out var card);
        card.InheritComponent<ProgressComponent>(this);
    }

    public override bool CanQuickInteract(Card card)
    {
        return card.CardId == "捞网";
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        Catch(slot.PeekCard(), out tip);
    }
}