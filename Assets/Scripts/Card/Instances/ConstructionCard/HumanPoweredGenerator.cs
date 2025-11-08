/// <summary>
/// 人力发电机
/// </summary>
public class HumanPoweredGenerator : ConstructionCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("人力发电", "踩轮子发电", EasyEvent_DontDestroy, Judge_Generate,
            () => 60,
            () => new()
            {
                { PlayerStateEnum.Hydration, -5 },
                { PlayerStateEnum.Sobriety, -6 }
            },
            () => new()
            {
                { EnvironmentStateEnum.Electricity, 10 }
            });
        base.RegisterCardEvents(); // 拆毁
    }

    private bool Judge_Generate(out string hint)
    {
        hint = string.Empty;

        if (GameEventManager.Instance.IsEventOngoing<MagneticStorm>())
        {
            hint = $"受行星磁暴影响，{CardName}无法为其供电";
            return false;
        }

        return true;
    }
}