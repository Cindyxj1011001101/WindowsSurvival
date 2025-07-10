using System.Collections.Generic;

/// <summary>
/// 背包运行时数据
/// </summary>
public class BagRuntimeData
{
    /// <summary>
    /// 列表的长度代表了背包当前的格子数量，每一项代表这一格里的卡牌内容
    /// </summary>
    public List<CardSlotRuntimeData> cardSlotsRuntimeData = new();
}