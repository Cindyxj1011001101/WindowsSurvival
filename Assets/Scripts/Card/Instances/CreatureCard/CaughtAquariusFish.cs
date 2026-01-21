/// <summary>
/// 被捉住的水瓶鱼
/// </summary>
[CardId("被捉住的水瓶鱼")]
public class CaughtAquariusFish : Card
{
    protected override void RegisterCardEvents()
    {
         AddCardEvent("放生", "放生水瓶鱼", Event_Release, Judge_Release);
    }

    protected override void OnLateConstructor()
    {
        // 被捉住的水瓶鱼的产物进度不会随时间增加
        progress.updateRate = 0;
    }

    private void Event_Release(CardEvent e)
    {
        DestroyThis();

        // 地点中增加一个水瓶鱼
        // 继承产物进度
        var card = CardFactory.CreateCard("水瓶鱼");
        card.InheritComponent<ProgressComponent>(this, out var progress);
        progress.updateRate = 1;
        TurnTo(card, GameManager.Instance.CurEnvironmentBag);
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