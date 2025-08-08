public class FoodScrap : Card
{
    public int RemainRound;
    private FoodScrap()
    {
        RemainRound=4;
        Events = new()
        {
            new Event("食用", "食用食物残渣", Event_Eat, null)
        };
    }

    public void Event_Eat(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 12);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -3);
        TimeManager.Instance.AddTime(15);
    }
    protected override System.Action OnUpdate => () =>
    {
        if (GameManager.Instance.CurEnvironmentBag.PlaceData.isInWater)
        {
            RemainRound--;
            if (RemainRound <= 0)
            {
                DestroyThis();
            }
        }
    };
}