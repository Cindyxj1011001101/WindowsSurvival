using System.Collections.Generic;

public class FoodScrap : Card
{
    public int remainRound = 4;
    private FoodScrap()
    {
        Events = new()
        {
            new Event("食用", "和鱼抢吃的", Event_Eat, null, () => 15,
            () => new Dictionary<PlayerStateEnum, float>() { { PlayerStateEnum.Fullness, 12 }, { PlayerStateEnum.San, -3 } }),
        };
    }

    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 12);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -3);
        TimeManager.Instance.AddTime(15);
    }
    protected override System.Action OnUpdate => () =>
    {
        if (remainRound <= 0 || GameManager.Instance.CurEnvironmentBag.PlaceData.isInWater) return;

        remainRound--;
        if (remainRound <= 0)
        {
            ShowTip("食物残渣被水冲走了");
            DestroyThis();
        }
    };
}