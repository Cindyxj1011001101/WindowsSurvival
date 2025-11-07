/// <summary>
/// 蠕动盛宴
/// </summary>
public class CreepFeast: Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 38 },
                { PlayerStateEnum.Sanity, -26 },
            });
    }
}   