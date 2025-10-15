/// <summary>
/// 电网故障。期间所有电器不可使用
/// </summary>
public class PowerNetworkFailure : GlobalEffect
{
    public PowerNetworkFailure(int duration) : base(duration)
    {
    }
}