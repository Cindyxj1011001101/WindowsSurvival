public enum EventType
{
    IntervalSettle,
    ChangeLoad, // 背包载重变化
    ChangeCardProperty, // 卡牌属性变化
    Move,//场景移动界面刷新
    RefreshPlayerState,//更新玩家状态数据
    RefreshEnvironmentState,//更新环境状态数据
    ChangeDiscoveryDegree, // 探索度变化
    ChangePlayerBagCards, // 玩家背包卡牌变化
    ChangeStudyProgress, // 研究进度变化
    UnlockRecipe, // 解锁合成配方
    GameOver, // 游戏结束
    Equip, // 穿上装备
    Unequip, // 卸下装备
    TriggerParagraph, // 触发对话
    DialogueCondition, // 触发对话条件
    ChangeWaterLevel, // 水平面变化
    ChangeTime, // 时间变化
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

public class ChangePlayerBagCardsArgs
{
    public Card card;
    public int add;
}

public class RefreshEnvironmentStateArgs
{
    public PlaceEnum place;
    public EnvironmentStateEnum stateEnum;
    public EnvironmentState stateValue;
    public bool hasCable;
    public PressureLevel pressureLevel;

    public RefreshEnvironmentStateArgs(PlaceEnum place, EnvironmentStateEnum stateEnum)
    {
        this.place = place;
        this.stateEnum = stateEnum;
    }
}

public class SubscribeActionArgs
{
    public string type;
    public string value;

    public SubscribeActionArgs(string t, string v)
    {
        type = t;
        value = v;
    }
}