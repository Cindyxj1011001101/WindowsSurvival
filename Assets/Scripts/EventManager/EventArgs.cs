public enum EventType
{
    Update,                             // 回合结算(15分钟一次)
    UpdateBegin,                        // 回合结算开始，记录快照
    FineUpdate,                         // 一分钟一次
    AnotherDay,                         // 跨天行为触发
    ChangeCardProperty,                 // 卡牌属性变化
    ChangeCurrentEnvironment,           // 改变当前地点
    RefreshPlayerState,                 // 更新玩家状态数据
    RefreshEnvironmentState,            // 更新环境状态数据
    AddRemoveCard,                      // 向添加/移除卡牌
    UnlockRecipe,                       // 解锁合成配方
    GameOver,                           // 游戏结束
    TriggerParagraph,                   // 触发对话
    DialogueCondition,                  // 触发对话条件
    StartStudy,                         // 开始研究
    StopStudy,                          // 停止研究
    ComplishStudy,                      // 完成研究
    RefreshStudyWindow,                 // 刷新研究窗口
    InterruptStudy,                     // 中断研究
    StartChangeTime,                    // 时间变化开始
    EndChangeTime,                      // 时间变化结束
    PickUpCard,                         // 拿起一张卡牌
    PutDownCard,                        // 放下一张卡牌
    StartSleeping,                      // 开始睡觉
    StopSleeping,                       // 停止睡觉
    CardNumChange,                      // 卡牌数量变化
    PlayerMove,                         // 玩家移动
    GameEventBegin,                     // 全局效果开始
    GameEventEnd,                       // 全局效果结束
    UpdateSunlight,                     // 恒星光照更新
    RefreshAnimator,                    // 更新动画器
    ChangeDisplayedCard,                // 切换详情窗口显示的卡牌
}

public class AddRemoveCardArgs
{
    public Card card;
    public int add;
    public Bag fromBag;
    public Bag toBag;

    public Bag AffectedBag
    {
        get
        {
            return add > 0 ? toBag : fromBag;
        }
    }
}

public class RefreshEnvironmentStateArgs
{
    public PlaceEnum place;
    public EnvironmentStateEnum stateEnum;
    public State stateValue;
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