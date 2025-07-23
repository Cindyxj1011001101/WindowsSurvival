/// <summary>
/// 被捉住的水瓶鱼
/// </summary>
public class CaughtAquariusFish : Card
{
    private CaughtAquariusFish()
    {
        Events = new()
        {
            new Event("放生", "放生水瓶鱼", Event_Release, Judge_Release),
        };
    }

    public void Event_Release()
    {
        DestroyThis();
        // 地点中增加一个水瓶鱼
        // 继承产物进度
        GameManager.Instance.AddCard("水瓶鱼", true).InheritComponent<ProgressComponent>(this);
    }

    public bool Judge_Release()
    {
        return GameManager.Instance.CurEnvironmentBag.PlaceData.isInWater;
    }
}