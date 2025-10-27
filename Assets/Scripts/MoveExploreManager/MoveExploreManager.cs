using System.Collections.Generic;
using UnityEngine;

public class MoveExploreManager : IManager
{
    public static MoveExploreManager Instance { get; } = new();

    // 探索额外消耗
    public BehaviourExtraEffects ExploreExtraEffects { get; private set; } = new();

    // 探索水域额外消耗
    public BehaviourExtraEffects ExploreInWaterExtraEffects { get; private set; } = new();

    // 移动额外消耗
    public BehaviourExtraEffects MoveExtraEffects { get; private set; } = new();

    // 移动到水域额外消耗
    public BehaviourExtraEffects MoveToWaterExtraEffects { get; private set; } = new();

    // 上次负重
    private int lastLoadLevel = -1;

    private List<(string reason, (float timeMultiplier, Dictionary<PlayerStateEnum, float> playerEffects) effect)> extraEffectsCausedByLoad = new()
    {
        { ("", (0f, new() { })) }, // 占位用
        { ("身上有点重", (0.25f, new() { })) },
        { ("身上很重", (1f, new() { { PlayerStateEnum.Health, -3 } })) },
        { ("身上太重了", (0f, new() { })) },
    };

    public void Init()
    {
        lastLoadLevel = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].StateLevel;
        InitBehaviourExtraEffects();
        // 切换场景
        ChangeEnv(GameDataManager.Instance.LastPlace);
        // 播放环境音乐
        SoundManager.Instance.PlayCurEnvironmentMusic();
        // 监听负重变化
        EventManager.Instance.AddListener<PlayerStateEnum>(EventType.RefreshPlayerState, OnLoadChange);
    }

    public void Reset()
    {
        lastLoadLevel = -1;
        MoveExtraEffects = new();
        MoveToWaterExtraEffects = new();
        ExploreExtraEffects = new();
        ExploreInWaterExtraEffects = new();
        EventManager.Instance.RemoveListener<PlayerStateEnum>(EventType.RefreshPlayerState, OnLoadChange);
    }

    private void InitBehaviourExtraEffects()
    {
        var data = GameDataManager.Instance.BehaviourExtraEffectsData;
        if (data.init)
        {
            MoveExtraEffects = data.moveExtraEffects;
            MoveToWaterExtraEffects = data.moveToWaterExtraEffects;
            ExploreExtraEffects = data.exploreExtraEffects;
            ExploreInWaterExtraEffects = data.exploreInWaterExtraEffects;
        }
        else
        {
            ExploreInWaterExtraEffects = new()
            {
                extraEffects = new Dictionary<string, (float timeMultiplier, Dictionary<PlayerStateEnum, float> playerEffects)>
                {
                    { "未装备氧气面罩", (+0.4f, new() { { PlayerStateEnum.Health, -4 } }) }
                }
            };
        }
    }

    #region 探索

    public void AddExploreExtraEffect(string reason, float timeMultiplier, Dictionary<PlayerStateEnum, float> playerEffects)
        => ExploreExtraEffects.AddEffect(reason, timeMultiplier, playerEffects);

    public void RemoveExploreExtraEffect(string reason)
        => ExploreExtraEffects.RemoveEffect(reason);

    public void AddMoveExtraEffect(string reason, float timeMultiplier, Dictionary<PlayerStateEnum, float> playerEffects)
        => MoveExtraEffects.AddEffect(reason, timeMultiplier, playerEffects);

    public void RemoveMoveExtraEffect(string reason)
        => MoveExtraEffects.RemoveEffect(reason);

    public void AddExploreInWaterExtraEffect(string reason, float timeMultiplier, Dictionary<PlayerStateEnum, float> playerEffects)
        => ExploreInWaterExtraEffects.AddEffect(reason, timeMultiplier, playerEffects);

    public void RemoveExploreInWaterExtraEffect(string reason)
        => ExploreInWaterExtraEffects.RemoveEffect(reason);

    public void AddMoveToWaterExtraEffect(string reason, float timeMultiplier, Dictionary<PlayerStateEnum, float> playerEffects)
        => MoveToWaterExtraEffects.AddEffect(reason, timeMultiplier, playerEffects);

    public void RemoveMoveToWaterExtraEffect(string reason)
        => MoveToWaterExtraEffects.RemoveEffect(reason);

    /// <summary>
    /// 得到探索当前地点的消耗
    /// </summary>
    /// <returns></returns>
    public (string desc, int time, Dictionary<PlayerStateEnum, float> playerEffects) GetExploreEffects()
    {
        var env = GameManager.Instance.CurEnvironmentBag;
        string desc = ExploreExtraEffects.GetDescription();
        int time = ExploreExtraEffects.GetFinalTime(env.PlaceData.exploreTime);
        Dictionary<PlayerStateEnum, float> playerEffects = ExploreExtraEffects.GetFinalPlayerEffects(new());

        // 对水域的探索额外消耗
        if (env.PlaceData.isInWater)
        {
            desc += ExploreInWaterExtraEffects.GetDescription();
            time = ExploreInWaterExtraEffects.GetFinalTime(time);
            playerEffects = ExploreInWaterExtraEffects.GetFinalPlayerEffects(playerEffects);
        }

        return (desc, time, playerEffects);
    }

    /// <summary>
    /// 得到移动到目标地点的消耗
    /// </summary>
    /// <param name="basicMoveTime"></param>
    /// <param name="targetPlace"></param>
    /// <returns></returns>
    public (string desc, int time, Dictionary<PlayerStateEnum, float> playerEffects)
        GetMoveEffects(int basicMoveTime, PlaceEnum targetPlace)
    {
        var targetEnv = GameManager.Instance.EnvironmentBags[targetPlace];

        string desc = MoveExtraEffects.GetDescription();
        int time = MoveExtraEffects.GetFinalTime(basicMoveTime);
        Dictionary<PlayerStateEnum, float> playerEffects = MoveExtraEffects.GetFinalPlayerEffects(new());

        // 前往水域的额外消耗
        if (targetEnv.PlaceData.isInWater)
        {
            desc += MoveToWaterExtraEffects.GetDescription();
            time = MoveToWaterExtraEffects.GetFinalTime(time);
            playerEffects = MoveToWaterExtraEffects.GetFinalPlayerEffects(playerEffects);
        }

        return (desc, time, playerEffects);
    }

    /// <summary>
    /// 得到地点内移动到目标位置的消耗
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    public (string desc, int time, Dictionary<PlayerStateEnum, float> playerEffects)
        GetMoveEffects(float targetPosition)
    {
        var dist = Mathf.Abs(Player.Instance.Coordinate.Position - targetPosition);

        var basicMoveTime = Mathf.CeilToInt(dist / Player.Instance.MoveSpeed);
        return GetMoveEffects(basicMoveTime, GameManager.Instance.CurEnvironmentBag.PlaceData.placeType);
    }

    /// <summary>
    /// 载重变化触发的移动探索额外消耗变化
    /// </summary>
    /// <param name="state"></param>
    private void OnLoadChange(PlayerStateEnum state)
    {
        if (state != PlayerStateEnum.Load || lastLoadLevel == StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].StateLevel) return;

        // 载重等级发生变化，更新额外消耗
        int currentLoadLevel = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].StateLevel;

        if (lastLoadLevel != 0)
        {
            // 移除上一个载重等级的额外消耗
            RemoveExploreExtraEffect(extraEffectsCausedByLoad[lastLoadLevel].reason);
            RemoveMoveExtraEffect(extraEffectsCausedByLoad[lastLoadLevel].reason);
        }

        if (currentLoadLevel != 0)
        {
            // 添加当前载重等级的额外消耗
            AddExploreExtraEffect(extraEffectsCausedByLoad[currentLoadLevel].reason, extraEffectsCausedByLoad[currentLoadLevel].effect.timeMultiplier, extraEffectsCausedByLoad[currentLoadLevel].effect.playerEffects);
            AddMoveExtraEffect(extraEffectsCausedByLoad[currentLoadLevel].reason, extraEffectsCausedByLoad[currentLoadLevel].effect.timeMultiplier, extraEffectsCausedByLoad[currentLoadLevel].effect.playerEffects);
        }

        lastLoadLevel = currentLoadLevel;
    }

    /// <summary>
    /// 能否进行探索移动
    /// </summary>
    /// <returns></returns>
    public bool CanMoveExplore() => StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].StateLevel < 3; // 负重过高时无法移动和探索

    /// <summary>
    /// 处理探索事件
    /// </summary>
    public void HandleExplore(out string tip, out List<Card> droppedCards)
    {
        tip = string.Empty;
        droppedCards = new List<Card>();

        if (!CanMoveExplore()) return;

        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("Click", "Explore"));

        var env = GameManager.Instance.CurEnvironmentBag;

        var disposableDropList = env.DisposableDropList;
        var deepExploreDropList = env.DeepExploreDropList;
        if (disposableDropList.IsEmpty && deepExploreDropList.IsEmpty)
        {
            Debug.Log("探索完全");
            return;
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("抽卡", true);

        (_, int time, Dictionary<PlayerStateEnum, float> playerEffects) = GetExploreEffects();

        // 玩家状态变化
        StateManager.Instance.ApplyPlayerStateChange(playerEffects);

        // 消耗时间
        TimeManager.Instance.AddTime(time);

        // 掉落卡牌
        HandeleExploreDrop(out tip, out droppedCards, disposableDropList, deepExploreDropList);
    }

    /// <summary>
    /// 处理探索掉落
    /// </summary>
    /// <param name="tip"></param>
    /// <param name="droppedCards"></param>
    private void HandeleExploreDrop(out string tip, out List<Card> droppedCards, DropList disposableDropList, DeepExploreDropList deepExploreDropList)
    {
        tip = string.Empty;
        droppedCards = new List<Card>();

        // 当一次性探索列表还有剩余
        if (!disposableDropList.IsEmpty)
        {
            // 掉落卡牌
            droppedCards = disposableDropList.RandomDrop(out tip);
        }
        // 如果还可以重复探索
        else if (!deepExploreDropList.IsEmpty)
        {
            droppedCards = deepExploreDropList.RandomDrop();
            if (droppedCards.IsNullOrEmpty())
            {
                tip = "地点资源缺乏，什么都没找到";
                SoundManager.Instance.PlaySound("错误提示");
            }
        }
    }
    #endregion

    #region 移动
    private void SetPlayerPosition(float targetPosition)
    {
        Player.Instance.Coordinate.SetPosition(targetPosition);
        EventManager.Instance.TriggerEvent(EventType.PlayerMove);
    }

    /// <summary>
    /// 移动到目标场景
    /// </summary>
    /// <param name="targetPlace"></param>
    /// <param name="basicMoveTime"></param>
    public void Move(PlaceEnum targetPlace, int basicMoveTime)
    {
        if (!CanMoveExplore()) return;

        var lastEnv = GameManager.Instance.CurEnvironmentBag;

        // 改变地点
        ChangeEnv(targetPlace);

        var env = GameManager.Instance.CurEnvironmentBag;

        //从切换后的场景单次探索列表中拿出回到原先场景的牌，加入当前场景背包
        Card passage = null;
        var passageCardId = $"从{env.PlaceName}到{lastEnv.PlaceName}";
        var droppedCards = env.DisposableDropList.CertainDrop(passageCardId);
        if (!droppedCards.IsNullOrEmpty())
        {
            passage = droppedCards[0];
            GameManager.Instance.AddCard(passage, false);
            passage.RefreshSlot();
        }

        // 将玩家实体添加到新地点
        lastEnv.RemoveEntity(Player.Instance);
        env.AddEntity(Player.Instance);

        // 玩家坐标设置在通道位置
        passage ??= env.FindCardOfId(passageCardId);
        if (passage != null)
        {
            passage.TryGetComponent<CoordinateComponent>(out var coordinate);
            SetPlayerPosition(coordinate.coordinate.Position);
        }

        // 移动消耗
        (_, int time, Dictionary<PlayerStateEnum, float> playerEffects) = GetMoveEffects(basicMoveTime, targetPlace);
        StateManager.Instance.ApplyPlayerStateChange(playerEffects);
        TimeManager.Instance.AddTime(time);

        // 触发事件
        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("EnterEnvironment", targetPlace.ToString()));
    }

    /// <summary>
    /// 地点内移动
    /// </summary>
    /// <param name="targetPosition"></param>
    public void Move(float targetPosition)
    {
        if (!CanMoveExplore()) return;

        var env = GameManager.Instance.CurEnvironmentBag;
        // 限制坐标范围
        targetPosition = Mathf.Clamp(targetPosition, env.PlaceData.minCoord, env.PlaceData.maxCoord);

        // 移动消耗
        (_, int time, Dictionary<PlayerStateEnum, float> playerEffects) =
            GetMoveEffects(targetPosition);

        // 执行移动
        SetPlayerPosition(targetPosition);
        StateManager.Instance.ApplyPlayerStateChange(playerEffects);
        TimeManager.Instance.AddTime(time);
    }

    /// <summary>
    /// 切换地点
    /// </summary>
    /// <param name="targetPlace">目标地点</param>
    private void ChangeEnv(PlaceEnum targetPlace)
    {
        var env = GameManager.Instance.CurEnvironmentBag;

        // 离开旧地点：关闭有循环音的卡牌的循环音
        foreach (var slot in env.Slots)
        {
            if (!slot.IsEmpty)
            {
                var card = slot.PeekCard();
                if (card.HasLoopSound)
                    card.OnLeaveEnvironment();
            }
        }

        env = GameManager.Instance.EnvironmentBags[targetPlace];

        // 进入新地点：播放新地点离有循环音的卡牌
        foreach (var slot in env.Slots)
        {
            if (!slot.IsEmpty)
            {
                var card = slot.PeekCard();
                if (card.HasLoopSound)
                    card.OnEnterEnvironment();
            }
        }

        // 播放新地点环境音
        SoundManager.Instance.PlayPlaceMusic(env);

        // 触发事件
        EventManager.Instance.TriggerEvent(EventType.ChangeEnv, env);
    }
    #endregion
}