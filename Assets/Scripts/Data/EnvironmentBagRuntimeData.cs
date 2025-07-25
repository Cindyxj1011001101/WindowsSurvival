using System.Collections.Generic;

public class EnvironmentBagRuntimeData : BagRuntimeData
{
    public bool init;
    public DisposableDropList disposableDropList = new();
    public RepeatableDropList repeatableDropList = new();
    public bool hasCable; // 是否铺设电缆
    public PressureLevel pressureLevel; // 压强等级
    public Dictionary<EnvironmentStateEnum, EnvironmentState> environmentStateDict = new(); // 环境状态
}