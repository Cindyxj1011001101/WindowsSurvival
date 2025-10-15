using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    private float addCardTransition = 0.4f;

    public PlayerBag PlayerBag { get; private set; }
    public Dictionary<PlaceEnum, EnvironmentBag> EnvironmentBags { get; private set; } = new();
    public EnvironmentBag CurEnvironmentBag { get; private set; }
    public EquipmentBag EquipmentBag { get; private set; }
    public Player Player { get; private set; }
    public Dictionary<PlaceEnum, PlaceData> PlaceDataDict { get; private set; } = new();

    public List<GlobalEffect> GlobalEffects { get; private set; } = new(); // 全局效果

    public bool IsCurrentEnvironment(Bag bag) => bag is EnvironmentBag env && env == CurEnvironmentBag;


    private void Awake()
    {
        instance = this;

        foreach (var placeData in Resources.LoadAll<PlaceData>("ScriptableObject/Place"))
        {
            PlaceDataDict.Add(placeData.placeType, placeData);
        }

        // 玩家背包
        PlayerBag = GameDataManager.Instance.PlayerBagData;
        // 所有环境背包
        EnvironmentBags = GameDataManager.Instance.EnvironmentBagDataDict;
        // 当前环境背包
        CurEnvironmentBag = EnvironmentBags[GameDataManager.Instance.LastPlace];
        EquipmentBag = GameDataManager.Instance.EquipmentData;
        Player = GameDataManager.Instance.PlayerData;

        // 全局效果
        GlobalEffects = GameDataManager.Instance.GlobalEffects;

        EventManager.Instance.AddListener<PlayerStateEnum>(EventType.RefreshPlayerState, OnLoadChange);
        EventManager.Instance.AddListener(EventType.UpdateBegin, OnCardUpdateBegin);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<PlayerStateEnum>(EventType.RefreshPlayerState, OnLoadChange);
        EventManager.Instance.RemoveListener(EventType.UpdateBegin, OnCardUpdateBegin);
        UpdateManager.Instance.GlobalEffectUpdate.RemoveListener(GlobalEffectUpdate);
        UpdateManager.Instance.CardUpdate.RemoveListener(CardUpdate);
    }

    #region 初始化

    private void Start()
    {
        TechnologyManager.Instance.Init();
        CraftManager.Instance.Init();
        InGameEventManager.Instance.Init();

        lastLoadLevel = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].StateLevel;
        PlayerBag.Init();
        EquipmentBag.Init();
        foreach (var bag in EnvironmentBags.Values)
        {
            bag.Init();
        }
        InitBehaviourExtraEffects();

        UpdateManager.Instance.GlobalEffectUpdate.AddListener(GlobalEffectUpdate);
        UpdateManager.Instance.CardUpdate.AddListener(CardUpdate);

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
    #endregion

    #region 全局效果
    public void AddGlobalEffect(GlobalEffect newEffect)
    {
        GlobalEffects.Add(newEffect);
        newEffect.OnBegin();
    }

    public bool ContainsGlobalEffect<T>() where T : GlobalEffect
    {
        return GlobalEffects.Find(g => g.GetType() == typeof(T)) != null;
    }

    private void GlobalEffectUpdate()
    {
        for (int i = GlobalEffects.Count - 1; i >= 0; i--)
        {
            var effect = GlobalEffects[i];
            effect.OnUpdate();
            if (effect.Duration <= 0)
            {
                effect.OnEnd();
                GlobalEffects.RemoveAt(i);
            }    
        }
    }
    #endregion

    #region 卡牌更新
    private void OnCardUpdateBegin()
    {
        foreach (var bag in EnvironmentBags.Values)
        {
            bag.OnUpdateBegin();
        }
        PlayerBag.OnUpdateBegin();
        EquipmentBag.OnUpdateBegin();
    }

    private void CardUpdate()
    {
        foreach (var bag in EnvironmentBags.Values)
        {
            bag.Update();
        }
        PlayerBag.Update();
        EquipmentBag.Update();
    }
    #endregion

    #region AddCard

    /// <summary>
    /// 添加卡牌到指定背包
    /// </summary>
    /// <param name="card"></param>
    /// <param name="targetBag"></param>
    public void AddCard(Card card, Bag targetBag)
    {
        card.Init();
        targetBag.AddCard(card);
    }

    public void AddCard(Card card, bool toPlayerBag)
    {
        if (toPlayerBag && PlayerBag.CanAddCard(card, out _))
            AddCard(card, PlayerBag);
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
            addCardTransition,
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
            addCardTransition,
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
        return AddCardsWithTween(toPlayerBag, startPos, cards.ToArray());
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

    public void AddCardsToTargetEnv(List<Card> cards, EnvironmentBag targetEnv)
    {
        if (targetEnv == CurEnvironmentBag)
        {
            AddCardsWithTween(cards, false, Vector2.up * 600);
        }
        else
        {
            foreach (var card in cards)
            {
                AddCard(card, targetEnv);
            }
        }
    }
    #endregion

    #region 装备
    /// <summary>
    /// 穿上装备
    /// </summary>
    /// <param name="equipment"></param>
    public void Equip(Card equipment, Vector3 startPos)
    {
        //// 找到卡牌位置
        //Transform transform = null;
        //if (equipment.Slot != null)
        //    transform = equipment.Slot.transform;
        //if (transform == null && equipment.Bag is InnerBag innerBag && innerBag.BelongedCard != null)
        //    transform = innerBag.BelongedCard.Transform;

        // 从原来的格子里移除
        equipment.SlotCards?.RemoveCard(equipment);

        // 添加到装备格子里
        AddCardWithTween(equipment, EquipmentBag, startPos); // transform 理论上不会为空
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
        string desc = ExploreExtraEffects.GetEffectsDescription();
        int time = ExploreExtraEffects.GetFinalTime(CurEnvironmentBag.PlaceData.exploreTime);
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

    public (string desc, int time, Dictionary<PlayerStateEnum, float> playerEffects)
        GetMoveEffects(int basicMoveTime, PlaceEnum targetPlace)
    {
        string desc = MoveExtraEffects.GetEffectsDescription();
        int time = MoveExtraEffects.GetFinalTime(basicMoveTime);
        Dictionary<PlayerStateEnum, float> playerEffects = MoveExtraEffects.GetFinalPlayerEffects(new());

        // 前往水域的额外消耗
        if (EnvironmentBags[targetPlace].PlaceData.isInWater)
        {
            desc += MoveToWaterExtraEffects.GetEffectsDescription();
            time = MoveToWaterExtraEffects.GetFinalTime(time);
            playerEffects = MoveToWaterExtraEffects.GetFinalPlayerEffects(playerEffects);
        }

        return (desc, time, playerEffects);
    }

    public (string desc, int time, Dictionary<PlayerStateEnum, float> playerEffects)
        GetMoveEffects(float targetPosition)
    {
        var dist = Mathf.Abs(Player.Coordinate.Position - targetPosition);

        var basicMoveTime = Mathf.CeilToInt(dist / Player.MoveSpeed);
        return GetMoveEffects(basicMoveTime, CurEnvironmentBag.PlaceData.placeType);
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

        var disposableDropList = CurEnvironmentBag.DisposableDropList;
        var repeatableDropList = CurEnvironmentBag.RepeatableDropList;
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
        var disposableDropList = CurEnvironmentBag.DisposableDropList;
        var repeatableDropList = CurEnvironmentBag.RepeatableDropList;

        // 当一次性探索列表还有剩余
        if (!disposableDropList.IsEmpty)
        {
            // 掉落卡牌
            droppedCards = disposableDropList.RandomDrop();
            if (droppedCards.IsNullOrEmpty())
                return;
        }
        // 如果还可以重复探索
        else if (!repeatableDropList.IsEmpty)
        {
            droppedCards = repeatableDropList.RandomDrop();
            if (droppedCards.IsNullOrEmpty())
            {
                tip = "地点资源缺乏，什么都没找到";
                SoundManager.Instance.PlaySound("错误提示");
                return;
            }
        }
    }
    #endregion

    #region 移动

    private void SetPlayerPosition(float targetPosition)
    {
        Player.Coordinate.SetPosition(targetPosition);
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

        var lastEnv = CurEnvironmentBag;

        // 改变地点
        ChangeEnv(targetPlace);

        //从切换后的场景单次探索列表中拿出回到原先场景的牌，加入当前场景背包
        Card passage = null;
        var passageCardId = $"从{CurEnvironmentBag.PlaceName}到{lastEnv.PlaceName}";
        var droppedCards = CurEnvironmentBag.DisposableDropList.CertainDrop(passageCardId);
        if (!droppedCards.IsNullOrEmpty())
        {
            passage = droppedCards[0];
            AddCard(passage, false);
            passage.RefreshSlot();
        }

        // 将玩家实体添加到新地点
        lastEnv.RemoveEntity(Player);
        CurEnvironmentBag.AddEntity(Player);

        // 玩家坐标设置在通道位置
        passage ??= CurEnvironmentBag.FindCardOfId(passageCardId);
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

        // 限制坐标范围
        targetPosition = Mathf.Clamp(targetPosition, CurEnvironmentBag.PlaceData.minCoord, CurEnvironmentBag.PlaceData.maxCoord);
        
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
        // 离开旧地点：关闭有循环音的卡牌的循环音
        foreach (var slot in CurEnvironmentBag.Slots)
        {
            if (!slot.IsEmpty)
            {
                var card = slot.PeekCard();
                if (card.HasLoopSound)
                    card.OnLeaveEnvironment();
            }
        }

        CurEnvironmentBag = EnvironmentBags[targetPlace];

        // 进入新地点：播放新地点离有循环音的卡牌
        foreach (var slot in CurEnvironmentBag.Slots)
        {
            if (!slot.IsEmpty)
            {
                var card = slot.PeekCard();
                if (card.HasLoopSound)
                    card.OnEnterEnvironment();
            }
        }

        // 播放新地点环境音
        SoundManager.Instance.PlayPlaceMusic(CurEnvironmentBag);

        // 触发事件
        EventManager.Instance.TriggerEvent(EventType.ChangeEnv, CurEnvironmentBag);
    }
    #endregion
}