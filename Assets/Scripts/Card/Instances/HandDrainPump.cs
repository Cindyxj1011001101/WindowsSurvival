
public class HandDrainPump : Card
{
    private HandDrainPump()
    {
        Events = new()
        {
            new Event("手压排水", "手压排水", Event_Drain, Judge_Drain),
        };
        
    }

    public void Event_Drain(out string tip)
    {
        tip=string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, -3);
        StateManager.Instance.ChangeWaterLevel(-7);
        TimeManager.Instance.AddTime(30);
    }
    public bool Judge_Drain(out string hint)
    {
        hint = string.Empty;
        //是否需要判断当前地点是否有水平面属性
        return true;
    }
}