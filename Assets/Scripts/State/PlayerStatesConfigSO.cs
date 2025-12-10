using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有玩家状态配置的容器
/// </summary>
[CreateAssetMenu(fileName = "PlayerStatesConfig", menuName = "Config/Player States Config")]
public class PlayerStatesConfigSO : ScriptableObject
{
    [Header("玩家状态配置列表")]
    public List<PlayerStateConfigSO> stateConfigs = new();

    /// <summary>
    /// 创建所有玩家状态
    /// </summary>
    public Dictionary<PlayerStateEnum, State> CreateAllPlayerStates()
    {
        var result = new Dictionary<PlayerStateEnum, State>();

        foreach (var config in stateConfigs)
        {
            if (config == null)
            {
                Debug.LogWarning("[PlayerStatesConfig] 存在空的状态配置引用");
                continue;
            }

            if (result.ContainsKey(config.stateType))
            {
                Debug.LogWarning($"[PlayerStatesConfig] 状态类型 {config.stateType} 重复配置");
                continue;
            }

            result.Add(config.stateType, config.CreateState());
        }

        return result;
    }
}
