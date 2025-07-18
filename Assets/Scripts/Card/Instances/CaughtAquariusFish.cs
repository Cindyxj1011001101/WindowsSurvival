/// <summary>
/// 被捉住的水瓶鱼
/// </summary>
public class CaughtAquariusFish : Card
{
    public CaughtAquariusFish()
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
            new Event("放生", "放生水瓶鱼", Event_Release, Judge_Release),
        };
        components = new()
        {
            { typeof(ProgressComponent), new ProgressComponent(5760, null) },
        };
    }

    public CaughtAquariusFish(int progress) : this()
    {
        TryGetComponent<ProgressComponent>(out var component);
        component.progress = progress;
    }

    public void Event_Release()
    {
        DestroyThis();
        // 地点中增加一个水瓶鱼
        TryGetComponent<ProgressComponent>(out var component);
        GameManager.Instance.AddCard(new AquariusFish(component.progress), false);
    }

    public bool Judge_Release()
    {
        return GameManager.Instance.CurEnvironmentBag.PlaceData.isInWater;
    }
}