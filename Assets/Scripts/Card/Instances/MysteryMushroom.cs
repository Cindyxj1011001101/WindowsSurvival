using UnityEngine;

/// <summary>
/// 谜样菇
/// </summary>
public class MysteryMushroom : Card
{
    private MysteryMushroom()
    {
        Events = new()
        {
            new CardEvent("吃掉", "吃掉谜样菇。会有奇怪的感觉", Event_Eat, null, () => 5,
            () => new()
            {
                { PlayerStateEnum.Hunger, +11 }
            }),
        };
    }

    private void Event_Eat(out string tip)
    {
        tip = string.Empty;
        // 销毁自身
        DestroyThis();
        // 播放音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01", true);
        // 应用状态变化
        StateManager.Instance.ApplyPlayerStateChange(Events[0].GetPlayerEffects());
        ApplyRandomEffects();
        // 消耗时间
        TimeManager.Instance.AddTime(Events[0].GetTimeEffect());
    }

    private void ApplyRandomEffects()
    {
        int r = Random.Range(0, 4);

        if (r == 1)
        {
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sanity, Random.Range(-14, 11));
        }
        else if (r == 2)
        {
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, Random.Range(-10, 9));
        }
        else if (r == 3)
        {
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Hydration, Random.Range(-24, 16));
        }
        else
        {
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Oxygen, Random.Range(-50, 31));
        }
    }
}
