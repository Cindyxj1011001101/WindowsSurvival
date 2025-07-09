using System.Collections.Generic;

/// <summary>
/// 背包运行时数据
/// </summary>
public class BagRuntimeData
{
    public int firstLoad; // 是否是第一次启动游戏

    ///// <summary>
    ///// 当前格子数量
    ///// </summary>
    //public int slotCount = 9;

    ///// <summary>
    ///// 
    ///// </summary>
    //public List<int> notEmptyCardSlotIndecis = new();

    /// <summary>
    /// 列表的长度代表了背包当前的格子数量，每一项代表这一格里的卡牌内容
    /// </summary>
    public List<CardSlotRuntimeData> cardSlotsRuntimeData = new()
    {
        // 初始有9个空格子
        new CardSlotRuntimeData(), // 1
        new CardSlotRuntimeData(), // 2
        new CardSlotRuntimeData(), // 3
        new CardSlotRuntimeData(), // 4
        new CardSlotRuntimeData(), // 5
        new CardSlotRuntimeData(), // 6
        new CardSlotRuntimeData(), // 7
        new CardSlotRuntimeData(), // 8
        new CardSlotRuntimeData(), // 9
    };
}