
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
        if (!GameManager.Instance.CurEnvironmentBag.PlaceData.isInSpacecraft)
        {
            hint = "该场景无水平面属性";
            return false;
        }
        else if(StateManager.Instance.WaterLevel.CurValue==0)
        {
            hint = "当前场景水平面为0";
            return false;
        }
        return true;
    }
}