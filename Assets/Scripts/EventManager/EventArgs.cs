using System;

public enum EventType
{
    ChangeState,
    IntervalSettle,
    AddDropCard,
    ChangeLoad, // 背包载重变化
    ChangeCardProperty, // 卡牌属性变化
    Move,//场景移动界面刷新
    RefreshState,//更新状态数据
    ChangeDiscoveryDegree, // 探索度变化
    ChangeTime, // 时间变化
    ChangePlayerBagCards, // 玩家背包卡牌变化
    GameOver, // 游戏结束
    EquipCard, // 装备卡牌
    UnequipCard, // 卸下卡牌
}

public class ChangeStateArgs
{
    public PlayerStateEnum state;
    public float value;

    public ChangeStateArgs(PlayerStateEnum s, float i)
    {
        state = s;
        value = i;
    }
}

public class ChangeLoadArgs
{
    public float currentLoad;
    public float maxLoad;
}

public class AddDropCardArgs
{
    public Drop drop;
    public bool ToPlayer;

    public AddDropCardArgs(Drop d, bool b)
    {
        drop = d;
        ToPlayer = b;
    }
}

public class ChangeDiscoveryDegreeArgs
{
    public PlaceEnum place;
    public float discoveryDegree;

    public ChangeDiscoveryDegreeArgs(PlaceEnum p, float d)
    {
        place = p;
        discoveryDegree = d;
    }
}

public class ChangeTimeArgs
{
    public DateTime currentTime;
    public int timeDelta;
}

public class ChangePlayerBagCardsArgs
{
    public CardInstance card;
    public int add;
}

#region 装备卡牌
public class EquipCardArgs
{
    public EquipmentType type;
    public CardInstance card;

    public EquipCardArgs(EquipmentType t, CardInstance c)
    {
        type = t;
        card = c;
    }
}
#endregion