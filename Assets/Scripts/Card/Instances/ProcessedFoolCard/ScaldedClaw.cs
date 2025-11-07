/// <summary>
/// 白灼触手
/// </summary>
public class ScaldedClaw : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 45,
            () => new()
            {
                { PlayerStateEnum.Hunger, 81 },
                { PlayerStateEnum.Sanity, -3 }
            });
    }
}