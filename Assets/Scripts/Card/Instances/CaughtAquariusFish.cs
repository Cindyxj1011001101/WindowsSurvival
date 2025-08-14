/// <summary>
/// 被捉住的水瓶鱼
/// </summary>
public class CaughtAquariusFish : Card
{
    private CaughtAquariusFish()
    {
        Events = new()
        {
            //new Event("放生", "放生水瓶鱼", Event_Release, Judge_Release),
        };
    }

    public void Event_Release(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        // 地点中增加一个水瓶鱼
        // 继承产物进度
        AddCard("水瓶鱼", true, out var card);
        card.InheritComponent<ProgressComponent>(this);
    }

    public bool Judge_Release(out string hint)
    {
        hint = string.Empty;
        if (!GameManager.Instance.CurEnvironmentBag.PlaceData.isInWater)
        {
            hint = "只能放生在水域环境";
            return false;
        }
        return true;
    }
}