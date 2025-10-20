/// <summary>
/// 熟触手
/// </summary>
public class CookedTentacle : CookableCard
{
    private CookedTentacle()
    {
        Events = new()
        {
            new CardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null, () => 30,
            () => new()
            {
                { PlayerStateEnum.Fullness, 24 },
                { PlayerStateEnum.San, -1 },
            })
        };
    }
}