using Newtonsoft.Json;

/// <summary>
/// 食物残渣
/// </summary>
public class FoodScrap : Card
{
    [JsonProperty] private int disappearCountdown = 4;

    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "和鱼抢吃的", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 12 },
                { PlayerStateEnum.Sanity, -3 }
            });
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (!GameManager.Instance.CurEnvironmentBag.PlaceData.isInWater) return;

        disappearCountdown--;
        if (disappearCountdown <= 0)
        {
            ShowTip("食物残渣被水冲走了");
            DestroyThis();
        }
    }
}