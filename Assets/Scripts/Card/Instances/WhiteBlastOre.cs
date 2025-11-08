/// <summary>
/// 白爆矿
/// </summary>
public class WhiteBlastOre : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("敲碎", "会产生少量氧气", EasyEvent_Destroy, null,
            () => 3,
            () => new()
            {
                { PlayerStateEnum.Oxygen, 80 }
            });
    }
}