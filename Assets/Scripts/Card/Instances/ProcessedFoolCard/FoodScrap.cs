public class FoodScrap : Card
{
    public int remainRound = 4;
    private FoodScrap()
    {
        Events = new()
        {
            new CardEvent("食用", "和鱼抢吃的", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 12 },
                { PlayerStateEnum.Sanity, -3 }
            }),
        };
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (remainRound <= 0 || GameManager.Instance.CurEnvironmentBag.PlaceData.isInWater) return;

        remainRound--;
        if (remainRound <= 0)
        {
            ShowTip("食物残渣被水冲走了");
            DestroyThis();
        }
    }
}