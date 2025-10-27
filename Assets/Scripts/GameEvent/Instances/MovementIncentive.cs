/// <summary>
/// 移动激励
/// </summary>
public class MovementIncentive : GameEvent
{
    public override string GetDetails()
    {
        return $"麦麦最近精神很好，连游泳和跑步都变快了不少。\n\n" +
               $"在接下来的一段时间里，麦麦{ColorManager.Colorize("-50%", ColorManager.Green)}移动时长。";
    }

    public override void OnTrigger()
    {
        GameManager.Instance.AddMoveExtraEffect("移动激励", -0.5f, null);
    }

    public override void OnEnd()
    {
        GameManager.Instance.RemoveMoveExtraEffect("移动激励");
    }
}
