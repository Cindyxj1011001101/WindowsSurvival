using System.Collections.Generic;

public class EnvironmentBagRuntimeData : BagRuntimeData
{
    public DisposableDropList disposableDropList = new();
    public RepeatableDropList repeatableDropList = new();
    public Dictionary<EnvironmentStateEnum, EnvironmentState> environmentStateDict = new(); // 环境状态
}