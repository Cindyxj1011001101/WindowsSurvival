using System.Collections.Generic;

/// <summary>
/// 被捉住的水瓶鱼
/// </summary>
public class CaughtAquariusFishWithProduct : Card
{
    private CaughtAquariusFishWithProduct()
    {
        Events = new()
        {
            new Event("饮用", "饮用水瓶鱼的育卵液", Event_Drink, null, null, 15,
            new Dictionary<PlayerStateEnum, float>() { { PlayerStateEnum.Thirst, 15 },{ PlayerStateEnum.Fullness, 4 } }),
            
            new Event("放生", "放生水瓶鱼", Event_Release, Judge_Release)
        };
    }

    public void Event_Drink(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        // 播放喝水的音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("喝_01", true);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, 15);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 4);
        TimeManager.Instance.AddTime(15);
    }

    public void Event_Release(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        // 地点中增加一个有产物的水瓶鱼
        AddCard("有产物的水瓶鱼", true);
    }

    public bool Judge_Release()
    {
        return GameManager.Instance.CurEnvironmentBag.PlaceData.isInWater;
    }
}