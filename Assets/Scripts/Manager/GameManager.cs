using DG.Tweening;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 探索、移动等行为的额外效果
/// </summary>
public class BehaviourExtraEffects
{
    // <原因，(最终时间倍率，玩家状态额外变化值)>
    public Dictionary<string, (float timeMultiplier, Dictionary<PlayerStateEnum, float> playerEffects)> extraEffects = new();

    [JsonIgnore]
    public float FinalTimeMultiplier
    {
        get
        {
            float multiplier = 1f;
            foreach (var (timeMultiplier, _) in extraEffects.Values)
            {
                multiplier *= 1 + timeMultiplier;
            }
            return multiplier;
        }
    }

    public void AddEffect(string reason, float finalTimeMultiplier, Dictionary<PlayerStateEnum, float> playerEffects)
    {
        if (extraEffects.ContainsKey(reason)) return; // 如果已经存在该原因的效果，则不添加
        extraEffects.Add(reason, (finalTimeMultiplier, playerEffects));
    }

    public void RemoveEffect(string reason)
    {
        if (extraEffects.ContainsKey(reason))
        {
            extraEffects.Remove(reason);
        }
    }

    public int GetFinalTime(int basicTime)
    {
        return Mathf.CeilToInt(basicTime * FinalTimeMultiplier);
    }

    public string GetEffectsDescription()
    {
        string desc = string.Empty;
        foreach (var (reason, (timeMultiplier, playerEffects)) in extraEffects)
        {
            desc += $"\n{reason}，时间额外消耗{timeMultiplier * 100}%";
            if (playerEffects.IsNullOrEmpty()) continue;
            foreach (var (state, delta) in playerEffects)
            {
                desc += $"，{StateManager.ParsePlayerState(state)}额外{(delta > 0 ? "+" : "")}{delta}";
            }
        }
        return desc.TrimEnd('\n');
    }

    public Dictionary<PlayerStateEnum, float> GetFinalPlayerEffects(Dictionary<PlayerStateEnum, float> currentEffects)
    {
        static void AddEffects(Dictionary<PlayerStateEnum, float> final, Dictionary<PlayerStateEnum, float> current)
        {
            if (current.IsNullOrEmpty()) return;
            foreach (var (state, delta) in current)
            {
                if (final.ContainsKey(state)) final[state] += delta;
                else final.Add(state, delta);
            }
        }
        Dictionary<PlayerStateEnum, float> finalEffects = new();
        foreach (var (_, playerEffects) in extraEffects.Values)
        {
            AddEffects(finalEffects, playerEffects);
        }
        AddEffects(finalEffects, currentEffects);
        return finalEffects;
    }
}

public enum PlaceEnum
{
    /// <summary>
    /// 动力舱
    /// </summary>
    PowerCabin,
    /// <summary>
    /// 驾驶室
    /// </summary>
    Cockpit,
    /// <summary>
    /// 维生舱
    /// </summary>
    LifeSupportCabin,
    /// <summary>
    /// 珊瑚礁海域
    /// </summary>
    CoralCoast,
    /// <summary>
    /// 织光藻墓园
    /// </summary>
    PhosphorTomb,
    /// <summary>
    /// 飞船外壳
    /// </summary>
    SpaceshipOuterHull
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    private float addCardAnimDuration = 0.4f;

    private PlayerBag playerBag;
    private Dictionary<PlaceEnum, EnvironmentBag> environmentBags = new();
    private EnvironmentBag curEnvironmentBag;
    private EquipmentBag equipmentBag;

    public PlayerBag PlayerBag => playerBag;
    public Dictionary<PlaceEnum, EnvironmentBag> EnvironmentBags => environmentBags;
    public EnvironmentBag CurEnvironmentBag => curEnvironmentBag;
    public EquipmentBag EquipmentBag => equipmentBag;

    private void Awake()
    {
        instance = this;
        // 玩家背包
        playerBag = GameDataManager.Instance.PlayerBagData;
        // 所有环境背包
        environmentBags = GameDataManager.Instance.EnvironmentBagDataDict;
        // 当前环境背包
        curEnvironmentBag = environmentBags[GameDataManager.Instance.LastPlace];
        equipmentBag = GameDataManager.Instance.EquipmentData;

        EventManager.Instance.AddListener<PlayerStateEnum>(EventType.RefreshPlayerState, OnLoadChange);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<PlayerStateEnum>(EventType.RefreshPlayerState, OnLoadChange);
    }

    private void Start()
    {
        Init();

        TechnologyManager.Instance.Init();
    }

    private void Init()
    {
        lastLoadLevel = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].StateLevel;
        playerBag.Init();
        equipmentBag.Init();
        foreach (var bag in environmentBags.Values)
        {
            bag.Init();
        }
        InitBehaviourExtraEffects();
        ChangeEnv(GameDataManager.Instance.LastPlace);
        SoundManager.Instance.PlayCurEnvironmentMusic();
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
    }

    public bool IsCurrentEnvironment(Bag bag) => bag is EnvironmentBag env && env == curEnvironmentBag;

    #region AddCard

    /// <summary>
    /// 添加卡牌到指定背包
    /// </summary>
    /// <param name="card"></param>
    /// <param name="targetBag"></param>
    public void AddCard(Card card, Bag targetBag)
    {
        card.StartUpdating();
        targetBag.AddCard(card);
    }

    public void AddCard(Card card, bool toPlayerBag)
    {
        if (toPlayerBag && playerBag.CanAddCard(card, out _))
            AddCard(card, playerBag);
        else
            AddCard(card, CurEnvironmentBag);
    }

    /// <summary>
    /// 添加卡牌到指定背包(结合动效)
    /// </summary>
    /// <param name="card"></param>
    /// <param name="targetBag"></param>
    /// <returns></returns>
    public Tween AddCardWithTween(Card card, Bag targetBag, Vector2 startPos)
    {
        AddCard(card, targetBag);

        return MFXUtility.MoveCard(
            card,
            1,
            startPos,
            addCardAnimDuration,
            onComplete: () =>
            {
                card.RefreshSlot();
            });
    }

    public Tween AddCardsWithTween(bool toPlayerBag, Vector2 startPos, params Card[] cards)
    {
        foreach (var card in cards)
        {
            AddCard(card, toPlayerBag);
        }

        return MFXUtility.MoveCards(
            cards,
            startPos,
            addCardAnimDuration,
            onComplete: (card) =>
            {
                card.RefreshSlot();
            });
    }

    public Tween AddCardWithTween(string cardId, bool toPlayerBag, Vector2 startPos, out Card card)
    {
        card = CardFactory.CreateCard(cardId);

        return AddCardWithTween(card, toPlayerBag, startPos);
    }

    public Tween AddCardWithTween(Card card, bool toPlayerBag, Vector2 startPos)
    {
        return AddCardsWithTween(toPlayerBag, startPos, card);
    }

    public Tween AddCardsWithTween(List<Card> cards, bool toPlayerBag, Vector2 startPos)
    {
        return AddCardsWithTween(toPlayerBag, startPos,cards.ToArray());
    }

    public Tween AddCardsWithTween(string cardId, int count, bool toPlayerBag, Vector2 startPos, out List<Card> cards)
    {
        cards = new();

        for (int i = 0; i < count; i++)
        {
            cards.Add(CardFactory.CreateCard(cardId));
        }

        return AddCardsWithTween(cards, toPlayerBag, startPos);
    }
    #endregion

    #region 装备
    /// <summary>
    /// 穿上装备
    /// </summary>
    /// <param name="equipment"></param>
    public void Equip(Card equipment)
    {
        // 找到卡牌位置
        Transform transform = null;
        if (equipment.Slot != null)
            transform = equipment.Slot.transform;
        if (transform == null && equipment.Bag is InnerBag innerBag && innerBag.BelongedCard != null)
            transform = innerBag.BelongedCard.Transform;

        // 从原来的格子里移除
        equipment.SlotCards.RemoveCard(equipment);

        // 添加到装备格子里
        AddCardWithTween(equipment, equipmentBag, transform.position); // transform 理论上不会为空
    }

    /// <summary>
    /// 脱下装备
    /// </summary>
    /// <param name="type"></param>
    public void Unequip(Card equipment)
    {
        Transform transform = equipment.Slot.transform;

        // 从装备格子中移除
        equipment.SlotCards.RemoveCard(equipment);

        // 添加到背包(优先)或环境中
        AddCardWithTween(equipment, true, transform.position); // transform 理论上不会为空
    }

    /// <summary>
    /// 判断能否装备
    /// </summary>
    /// <param name="equipment"></param>
    /// <returns></returns>
    public bool CanEquip(Card equipment, out string tip)
    {
        return EquipmentBag.CanAddCard(equipment, out tip);
    }
    #endregion

    #region 探索
    // 探索额外消耗
    public BehaviourExtraEffects ExploreExtraEffects { get; private set; } = new();

    // 探索水域额外消耗
    public BehaviourExtraEffects ExploreInWaterExtraEffects { get; private set; } = new()
    {
        extraEffects = new Dictionary<string, (float timeMultiplier, Dictionary<PlayerStateEnum, float> playerEffects)>
        {
            { "未装备氧气面罩", (+0.4f, new() { { PlayerStateEnum.Health, -4 } }) }
        }
    };

    // 移动额外消耗
    public BehaviourExtraEffects MoveExtraEffects { get; private set; } = new();

    // 移动到水域额外消耗
    public BehaviourExtraEffects MoveToWaterExtraEffects { get; private set; } = new();

    private int lastLoadLevel = -1;

    private List<(string reason, (float timeMultiplier, Dictionary<PlayerStateEnum, float> playerEffects) effect)> extraEffectsCausedByLoad = new()
    {
        { ("", (0f, new() { })) }, // 占位用
        { ("身上有点重", (0.25f, new() { })) },
        { ("身上很重", (1f, new() { { PlayerStateEnum.Health, -3 } })) },
        { ("身上太重了", (0f, new() { })) },
    };

    public void AddExploreExtraEffect(string reason, float timeMultiplier, Dictionary<PlayerStateEnum, float> playerEffects)
    {
        ExploreExtraEffects.AddEffect(reason, timeMultiplier, playerEffects);
    }

    public void RemoveExploreExtraEffect(string reason)
    {
        ExploreExtraEffects.RemoveEffect(reason);
    }

    public void AddMoveExtraEffect(string reason, float timeMultiplier, Dictionary<PlayerStateEnum, float> playerEffects)
    {
        MoveExtraEffects.AddEffect(reason, timeMultiplier, playerEffects);
    }

    public void RemoveMoveExtraEffect(string reason)
    {
        MoveExtraEffects.RemoveEffect(reason);
    }

    public void AddExploreInWaterExtraEffect(string reason, float timeMultiplier, Dictionary<PlayerStateEnum, float> playerEffects)
    {
        ExploreInWaterExtraEffects.AddEffect(reason, timeMultiplier, playerEffects);
    }

    public void RemoveExploreInWaterExtraEffect(string reason)
    {
        ExploreInWaterExtraEffects.RemoveEffect(reason);
    }

    public void AddMoveToWaterExtraEffect(string reason, float timeMultiplier, Dictionary<PlayerStateEnum, float> playerEffects)
    {
        MoveToWaterExtraEffects.AddEffect(reason, timeMultiplier, playerEffects);
    }

    public void RemoveMoveToWaterExtraEffect(string reason)
    {
        MoveToWaterExtraEffects.RemoveEffect(reason);
    }

    public (string desc, int time, Dictionary<PlayerStateEnum, float> playerEffects) GetExploreEffects()
    {
        string desc = "探索该地点" + ExploreExtraEffects.GetEffectsDescription();
        int time = ExploreExtraEffects.GetFinalTime(curEnvironmentBag.PlaceData.exploreTime);
        Dictionary<PlayerStateEnum, float> playerEffects = ExploreExtraEffects.GetFinalPlayerEffects(new());

        // 对水域的探索额外消耗
        if (CurEnvironmentBag.PlaceData.isInWater)
        {
            desc += ExploreInWaterExtraEffects.GetEffectsDescription();
            time = ExploreInWaterExtraEffects.GetFinalTime(time);
            playerEffects = ExploreInWaterExtraEffects.GetFinalPlayerEffects(playerEffects);
        }

        return (desc, time, playerEffects);
    }

    public (string desc, int time, Dictionary<PlayerStateEnum, float> playerEffects) GetMoveEffects(int basicMoveTime, PlaceEnum targetPlace)
    {
        string desc = "前往" + ParsePlaceEnum(targetPlace) + MoveExtraEffects.GetEffectsDescription();
        int time = MoveExtraEffects.GetFinalTime(basicMoveTime);
        Dictionary<PlayerStateEnum, float> playerEffects = MoveExtraEffects.GetFinalPlayerEffects(new());

        // 前往水域的额外消耗
        if (environmentBags[targetPlace].PlaceData.isInWater)
        {
            desc += MoveToWaterExtraEffects.GetEffectsDescription();
            time = MoveToWaterExtraEffects.GetFinalTime(time);
            playerEffects = MoveToWaterExtraEffects.GetFinalPlayerEffects(playerEffects);
        }

        return (desc, time, playerEffects);
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

    public bool CanMoveExplore() => StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].StateLevel < 3;

    /// <summary>
    /// 处理探索事件
    /// </summary>
    public void HandleExplore(out string tip, out List<Card> droppedCards)
    {
        tip = string.Empty;
        droppedCards = new List<Card>();

        if (!CanMoveExplore()) return;

        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("Click", "Explore"));

        var disposableDropList = curEnvironmentBag.DisposableDropList;
        var repeatableDropList = curEnvironmentBag.RepeatableDropList;
        if (disposableDropList.IsEmpty && repeatableDropList.IsEmpty)
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
        HandeleExploreDrop(out tip, out droppedCards);
    }

    private void HandeleExploreDrop(out string tip, out List<Card> droppedCards)
    {
        tip = string.Empty;
        droppedCards = new List<Card>();
        var disposableDropList = curEnvironmentBag.DisposableDropList;
        var repeatableDropList = curEnvironmentBag.RepeatableDropList;

        // 当一次性探索列表还有剩余
        if (!disposableDropList.IsEmpty)
        {
            // 掉落卡牌
            droppedCards = disposableDropList.RandomDrop();
            if (droppedCards.IsNullOrEmpty())
            {
                tip = "什么也没有得到";
                return;
            }

            // 探索度变化
            EventManager.Instance.TriggerEvent(EventType.ChangeDiscoveryDegree, (curEnvironmentBag.DiscoveryDegree, curEnvironmentBag.ExploreCompleted));
        }
        // 如果还可以重复探索
        else if (!repeatableDropList.IsEmpty)
        {
            droppedCards = repeatableDropList.RandomDrop();
            if (droppedCards.IsNullOrEmpty())
            {
                tip = "什么也没有得到";
                return;
            }
        }
    }
    #endregion

    /// <summary>
    /// 移动到目标场景
    /// </summary>
    /// <param name="targetPlace"></param>
    /// <param name="basicMoveTime"></param>
    public void Move(PlaceEnum targetPlace, int basicMoveTime)
    {
        if (!CanMoveExplore()) return;

        ChangeEnv(targetPlace);

        (_, int time, Dictionary<PlayerStateEnum, float> playerEffects) = GetMoveEffects(basicMoveTime, targetPlace);

        // 移动消耗
        StateManager.Instance.ApplyPlayerStateChange(playerEffects);
        TimeManager.Instance.AddTime(time);
    }

    /// <summary>
    /// 变化场景
    /// </summary>
    /// <param name="targetPlace"></param>
    private void ChangeEnv(PlaceEnum targetPlace)
    {
        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("EnterEnvironment", targetPlace.ToString()));
        //拿到原先场景是哪个
        PlaceEnum lastPlace = curEnvironmentBag.PlaceData.placeType;

        SoundManager.Instance.PlayPlaceMusic(environmentBags[targetPlace]);
        
        // 离开旧地点：关闭有循环音的卡牌的循环音
        foreach (var slot in curEnvironmentBag.Slots)
        {
            if (!slot.IsEmpty)
            {
                var card = slot.PeekCard();
                if (card.HasLoopSound) 
                    card.OnLeaveEnvironment();
            }
        }

        curEnvironmentBag = environmentBags[targetPlace];
        
        // 进入新地点：播放新地点离有循环音的卡牌
        foreach (var slot in curEnvironmentBag.Slots)
        {
            if (!slot.IsEmpty)
            {
                var card = slot.PeekCard();
                if (card.HasLoopSound)
                    card.OnEnterEnvironment();
            }
        }

        //从切换后的场景单次探索列表中拿出必定回到原先场景的牌，加入当前场景背包
        var door = curEnvironmentBag.DisposableDropList.CertainDrop($"从{ParsePlaceEnum(targetPlace)}到{ParsePlaceEnum(lastPlace)}");
        if (!door.IsNullOrEmpty())
        {
            AddCard(door[0], false);
            door[0].RefreshSlot();
        }

        EventManager.Instance.TriggerEvent(EventType.Move, curEnvironmentBag);
    }

    public static string ParsePlaceEnum(PlaceEnum place)
    {
        return place switch
        {
            PlaceEnum.PowerCabin => "动力舱",
            PlaceEnum.Cockpit => "驾驶室",
            PlaceEnum.LifeSupportCabin => "维生舱",
            PlaceEnum.CoralCoast => "珊瑚礁海域",
            PlaceEnum.PhosphorTomb => "织光藻墓园",
            PlaceEnum.SpaceshipOuterHull => "飞船外壳",
            _ => null,
        };
    }
}