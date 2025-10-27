using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 探索、移动等行为的额外效果
/// </summary>
public class BehaviourExtraEffects
{
    // <原因，(最终时间倍率，玩家状态额外变化值)>
    public Dictionary<string, (float timeMultiplier, Dictionary<PlayerStateEnum, float> playerEffects)> extraEffects = new();

    [JsonIgnore]
    public float FinalTimeMultiplier
    {
        get
        {
            float multiplier = 1f;
            foreach (var (timeMultiplier, _) in extraEffects.Values)
            {
                multiplier *= 1 + timeMultiplier;
            }
            return multiplier;
        }
    }

    public void AddEffect(string reason, float finalTimeMultiplier, Dictionary<PlayerStateEnum, float> playerEffects)
    {
        if (extraEffects.ContainsKey(reason)) return; // 如果已经存在该原因的效果，则不添加
        extraEffects.Add(reason, (finalTimeMultiplier, playerEffects));
    }

    public void RemoveEffect(string reason)
    {
        if (extraEffects.ContainsKey(reason))
        {
            extraEffects.Remove(reason);
        }
    }

    public int GetFinalTime(int basicTime)
    {
        return Mathf.CeilToInt(basicTime * FinalTimeMultiplier);
    }

    public string GetDescription()
    {
        if (extraEffects.IsNullOrEmpty()) return string.Empty;

        var desc = new StringBuilder();
        desc.AppendLine();
        desc.AppendLine();
        foreach (var (reason, (timeMultiplier, playerEffects)) in extraEffects)
        {
            var str = $"{(timeMultiplier > 0 ? "+" : "")}{timeMultiplier * 100}%";
            str = ColorManager.Colorize(str, timeMultiplier > 0 ? ColorManager.Red : ColorManager.Green);
            desc.AppendLine($"{ColorManager.Colorize($"{reason}:", ColorManager.Yellow)}");
            desc.AppendLine($"  - 时间消耗 {str}");

            if (playerEffects.IsNullOrEmpty()) continue;

            foreach (var (state, delta) in playerEffects)
            {
                str = $"{(delta > 0 ? "+" : "")}{delta}";
                str = ColorManager.Colorize(str, delta < 0 ? ColorManager.Red : ColorManager.Green);
                desc.AppendLine($"  - {StateManager.ParsePlayerState(state)} {str}");
            }
        }
        return desc.ToString().TrimEnd('\n');
    }

    public Dictionary<PlayerStateEnum, float> GetFinalPlayerEffects(Dictionary<PlayerStateEnum, float> currentEffects)
    {
        static void AddEffects(Dictionary<PlayerStateEnum, float> final, Dictionary<PlayerStateEnum, float> current)
        {
            if (current.IsNullOrEmpty()) return;
            foreach (var (state, delta) in current)
            {
                if (final.ContainsKey(state)) final[state] += delta;
                else final.Add(state, delta);
            }
        }
        Dictionary<PlayerStateEnum, float> finalEffects = new();
        foreach (var (_, playerEffects) in extraEffects.Values)
        {
            AddEffects(finalEffects, playerEffects);
        }
        AddEffects(finalEffects, currentEffects);
        return finalEffects;
    }
}