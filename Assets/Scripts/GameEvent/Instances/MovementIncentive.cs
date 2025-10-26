/// <summary>
/// 移动激励
/// </summary>
public class MovementIncentive : GameEvent
{
    public override string GetDetails()
    {
        return @"麦麦最近精神很好，连游泳和跑步都变快了不少。
                 在接下来的一段时间里，麦麦-50%移动时长。";
    }

    public override void OnTrigger()
    {
        
    }
}
