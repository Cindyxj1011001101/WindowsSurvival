using System.Collections.Generic;

public class EnvironmentBagRuntimeData : BagRuntimeData
{
    public List<Drop> remainingDrops = new(); // 剩余的一次性掉落列表
    public Dictionary<EnvironmentStateEnum, EnvironmentState> environmentStateDict = new(); // 环境状态
}