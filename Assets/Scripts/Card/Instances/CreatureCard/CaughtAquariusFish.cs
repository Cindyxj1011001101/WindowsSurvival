/// <summary>
/// 被捉住的水瓶鱼
/// </summary>
public class CaughtAquariusFish : Card
{
    protected override void RegisterCardEvents()
    {
        // AddEvent("放生", "放生水瓶鱼", Event_Release, Judge_Release);
    }

    protected override void OnLateConstructor()
    {
        // 被捉住的水瓶鱼的产物进度不会随时间增加
        progress.updateRate = 0;
    }

    private void Event_Release(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        // 地点中增加一个水瓶鱼
        // 继承产物进度
        TurnTo("水瓶鱼", GameManager.Instance.CurEnvironmentBag, out var card);
        card.InheritComponent<ProgressComponent>(this, out var progress);
        progress.updateRate = 1;
    }

    private bool Judge_Release(out string hint)
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