using UnityEngine;

/// <summary>
/// 被捉住的水瓶鱼
/// </summary>
public class CaughtAquariusFishWithProduct : Card
{
    public override Sprite CardImage => Resources.Load<Sprite>("CardImage/有产物的被捉住的水瓶鱼");
    public CaughtAquariusFishWithProduct()
    {
        //初始化参数
        cardName = "被捉住的水瓶鱼";
        cardDesc = "一只水瓶鱼，其怀孕时体内的育卵液是重要的淡水来源。";
        cardType = CardType.Creature;
        maxStackNum = 5;
        moveable = true;
        weight = 1.1f;
        events = new()
        {
            new Event("饮用", "饮用水瓶鱼", Event_Drink, null),
            new Event("放生", "放生水瓶鱼", Event_Release, Judge_Release),
        };
    }

    public void Event_Drink()
    {
        DestroyThis();
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Thirst, 15));
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, 4));
        TimeManager.Instance.AddTime(15);
    }

    public void Event_Release()
    {
        DestroyThis();
        // 地点中增加一个水瓶鱼
        GameManager.Instance.AddCard(new AquariusFishWithProduct(), false);
    }

    public bool Judge_Release()
    {
        return GameManager.Instance.CurEnvironmentBag.PlaceData.isInWater;
    }
}