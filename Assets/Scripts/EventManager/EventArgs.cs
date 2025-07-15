using System;

public enum EventType
{
    ChangeState,
    IntervalSettle,
    AddDropCard,
    ChangeLoad, // 背包载重变化
    ChangeCardProperty, // 卡牌属性变化
    Move,//场景移动界面刷新
    RefreshPlayerState,//更新玩家状态数据
    RefreshEnvironmentState,//更新环境状态数据
    ChangeDiscoveryDegree, // 探索度变化
    //ChangeTime, // 时间变化
    ChangePlayerBagCards, // 玩家背包卡牌变化
    ChangeStudyProgress, // 研究进度变化
    UnlockRecipe, // 解锁合成配方
    GameOver, // 游戏结束
    CurEnvironmentChangeState, // 当前环境状态变化
    EquipCard,
    UnequipCard,

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

//public class ChangeLoadArgs
//{
//    public float currentLoad;
//    public float maxLoad;
//}

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

//public class ChangeTimeArgs
//{
//    public DateTime currentTime;
//    public int timeDelta;
//}

public class ChangePlayerBagCardsArgs
{
    public CardInstance card;
    public int add;
}

//#region 装备卡牌
//public class EquipCardArgs
//{
//    public EquipmentType type;
//    public CardInstance card;

//    public EquipCardArgs(EquipmentType t, CardInstance c)
//    {
//        type = t;
//        card = c;
//    }
//}
//#endregion

#region 环境状态变化
public class ChangeEnvironmentStateArgs
{
    public PlaceEnum place;
    public EnvironmentStateEnum state;
    public float value;

    public ChangeEnvironmentStateArgs(EnvironmentStateEnum s, float i)
    {
        place = GameManager.Instance.CurEnvironmentBag.PlaceData.placeType;
        state = s;
        value = i;
    }
}
#endregion

public class RefreshEnvironmentStateArgs
{
    public PlaceEnum place;
    public EnvironmentStateEnum state;

    public RefreshEnvironmentStateArgs(PlaceEnum p, EnvironmentStateEnum s)
    {
        place = p;
        state = s;
    }
}