/// <summary>
/// 人力发电机
/// </summary>
public class HumanPoweredGenerator : ConstructionCard
{
    private HumanPoweredGenerator()
    {
        Events = new()
        {
            new CardEvent("人力发电", "踩轮子发电", (out string s) => EasyEvent(out s, destroyThis: false), Judge_Generate, () => 60,
            () => new()
            {
                { PlayerStateEnum.Hydration, -5 },
                { PlayerStateEnum.Sobriety, -6 }
            },
            () => new()
            {
                { EnvironmentStateEnum.Electricity, 10 }
            })
        };
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