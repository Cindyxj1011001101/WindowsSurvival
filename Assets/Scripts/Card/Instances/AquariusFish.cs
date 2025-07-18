using UnityEngine;

/// <summary>
/// 水瓶鱼
/// </summary>
public class AquariusFish : Card
{
    public AquariusFish()
    {
        cardName = "水瓶鱼";
        cardDesc = "水瓶鱼是白塔星浅海特有的卵胎生鱼类，其雄鱼体型不足雌鱼0.1% ，终生附着在雌鱼泄殖腔附近。怀孕期间，雌鱼通过腹腔生物渗透膜从海水中过滤淡水，混合蛋白质形成富含营养的琥珀色育卵液。其半透明腹腔可见游动的胚胎群。";
        cardType = CardType.Creature;
        maxStackNum = 5;
        moveable = false;
        weight = 1.1f;
        events = new()
        {
            new Event("用捕网捉", "用捕网捉水瓶鱼", Event_CatchByNet, Judge_CatchByNet),
            new Event("用手捉", "用手捉水瓶鱼", Event_CatchByHand, null),
        };
        components = new()
        {
            { typeof(ProgressComponent), new ProgressComponent(5760) },
        };
    }

    public AquariusFish(int progress) : this()
    {
        TryGetComponent<ProgressComponent>(out var component);
        component.progress = progress;
    }

    private void OnProgressFull()
    {
        DestroyThis();
        GameManager.Instance.AddCard(new AquariusFishWithProduct(), true);
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
        TryGetComponent<ProgressComponent>(out var component);
        GameManager.Instance.AddCard(new CaughtAquariusFish(component.progress), true);
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
            TryGetComponent<ProgressComponent>(out var component);
            GameManager.Instance.AddCard(new CaughtAquariusFish(component.progress), true);
        }
    }
    #endregion

    protected override System.Action OnUpdate => () =>
    {
        TryGetComponent<ProgressComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval, OnProgressFull);
    };
}