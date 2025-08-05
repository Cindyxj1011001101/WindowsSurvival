using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System.Collections.Generic;
using UnityEngine;

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
    private EnvironmentBagWindow envWindow;

    public PlayerBag PlayerBag => playerBag;
    public Dictionary<PlaceEnum, EnvironmentBag> EnvironmentBags => environmentBags;
    public EnvironmentBag CurEnvironmentBag => curEnvironmentBag;
    public EquipmentBag EquipmentBag => equipmentBag;

    private void Awake()
    {
        instance = this;
        // 玩家背包
        playerBag = FindObjectOfType<PlayerBag>(true);
        // 所有环境背包
        foreach (var bag in FindObjectsOfType<EnvironmentBag>(true))
        {
            environmentBags.Add(bag.PlaceData.placeType, bag);
        }
        // 当前环境背包
        curEnvironmentBag = environmentBags[GameDataManager.Instance.LastPlace];
        equipmentBag = FindObjectOfType<EquipmentBag>(true);

        envWindow = FindObjectOfType<EnvironmentBagWindow>(true);
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        playerBag.Init();
        equipmentBag.Init();
        foreach (var bag in environmentBags.Values)
        {
            bag.Init();
        }
        Move(GameDataManager.Instance.LastPlace);
        SoundManager.Instance.PlayCurEnvironmentMusic();
    }

    private void AddCard(Card card, bool toPlayerBag)
    {
        // 卡牌的属性开始随时间变化
        card.StartUpdating();

        if (toPlayerBag && playerBag.CanAddCard(card, out _))
        {
            if (!WindowsManager.Instance.IsWindowOpen("PlayerBag"))
                WindowsManager.Instance.OpenWindow("PlayerBag");
            playerBag.AddCard(card);
        }
        else
        {
            if (!WindowsManager.Instance.IsWindowOpen("EnvironmentBag"))
                WindowsManager.Instance.OpenWindow("EnvironmentBag");
            curEnvironmentBag.AddCard(card);
        }
    }

    public void AddCardWithTween(Card card, Vector2 startPos, bool toPlayerBag)
    {
        AddCard(card, toPlayerBag);

        CardTweenUtility.MoveCard(
            card,
            1,
            startPos,
            card.Slot.transform.position,
            addCardAnimDuration,
            onComplete: () =>
            {
                card.Slot.RefreshCurrentDisplay();
            });
    }

    public Card AddCardWithTween(string cardId, Vector2 startPos, bool toPlayerBag)
    {
        var card = CardFactory.CreateCard(cardId);
        AddCardWithTween(card, startPos, toPlayerBag);
        return card;
    }

    public List<Card> AddCardsWithTween(string cardId, int count, Vector2 startPos, bool toPlayerBag)
    {
        List<Card> cards = new();

        for (int i = 0; i < count; i++)
        {
            cards.Add(CardFactory.CreateCard(cardId));
        }

        AddCardsWithTween(cards, startPos, toPlayerBag);

        return cards;
    }

    public void AddCardsWithTween(List<Card> cards, Vector2 startPos, bool toPlayerBag)
    {
        foreach (var card in cards)
        {
            AddCard(card, toPlayerBag);
        }

        CardTweenUtility.MoveCardsWithDelay(
            cards,
            startPos,
            addCardAnimDuration,
            onComplete: (card) =>
            {
                card.Slot.RefreshCurrentDisplay();
            });
    }

    #region 装备
    /// <summary>
    /// 穿上装备
    /// </summary>
    /// <param name="equipment"></param>
    public void Equip(Card equipment)
    {
        // 从原来的格子里移除
        var originalSlot = equipment.Slot;
        originalSlot.RemoveCard(equipment);
        originalSlot.RefreshCurrentDisplay();

        // 打开装备窗口
        if (!WindowsManager.Instance.IsWindowOpen("Equipment"))
            WindowsManager.Instance.OpenWindow("Equipment");

        // 添加到装备格子里
        EquipmentBag.AddCard(equipment);
        CardTweenUtility.MoveCard(
            equipment,
            1,
            originalSlot.transform.position,
            equipment.Slot.transform.position,
            onComplete: () =>
            {
                equipment.Slot.RefreshCurrentDisplay();
            }
            );
    }

    /// <summary>
    /// 脱下装备
    /// </summary>
    /// <param name="type"></param>
    public void Unequip(Card equipment)
    {
        // 从装备格子中移除
        var originalSlot = equipment.Slot;
        originalSlot.RemoveCard(equipment);
        originalSlot.RefreshCurrentDisplay();

        // 添加到背包(优先)或环境中
        AddCardWithTween(equipment, originalSlot.transform.position, true);
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

    public (string desc, int time, Dictionary<PlayerStateEnum, float> playerEffects,
        Dictionary<EnvironmentStateEnum, float> envEffects) GetExploreEffects()
    {
        string desc = "";
        int time = curEnvironmentBag.explorationTime;
        Dictionary<PlayerStateEnum, float> playerEffects = new();
        Dictionary<EnvironmentStateEnum, float> envEffects = new();
        switch (curEnvironmentBag.PlaceData.placeType)
        {
            case PlaceEnum.PowerCabin:
            case PlaceEnum.Cockpit:
            case PlaceEnum.LifeSupportCabin:
                break;
            case PlaceEnum.CoralCoast:
                desc = "最好佩戴上氧气面罩";
                // 如果没有佩戴氧气面罩
                if (equipmentBag.FindCardOfName("氧气面罩") == null)
                {
                    // 探索时间+40%
                    time += Mathf.CeilToInt(curEnvironmentBag.explorationTime * .4f);
                    // 健康值-4
                    playerEffects.Add(PlayerStateEnum.Health, -4);
                }
                break;
        }

        return (desc, time, playerEffects, envEffects);
    }

    /// <summary>
    /// 处理探索事件
    /// </summary>
    /// <param name="startPos">抽牌动效的开始位置，即环境窗口牌堆的位置</param>
    public void HandleExplore(out string tip)
    {
        tip = string.Empty;
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

        int explorationTime = curEnvironmentBag.explorationTime;

        switch (curEnvironmentBag.PlaceData.placeType)
        {
            case PlaceEnum.PowerCabin:
            case PlaceEnum.Cockpit:
            case PlaceEnum.LifeSupportCabin:
                break;
            case PlaceEnum.CoralCoast:
                // 如果没有佩戴氧气面罩
                if (equipmentBag.FindCardOfName("氧气面罩") == null)
                {
                    // 探索时间+40%
                    explorationTime += Mathf.CeilToInt(curEnvironmentBag.explorationTime * .4f);
                    // 健康值-4
                    StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -4);
                }
                break;
        }

        // 消耗时间
        TimeManager.Instance.AddTime(explorationTime);

        // 掉落卡牌
        HandeleExploreDrop(out tip);
    }

    private void HandeleExploreDrop(out string tip)
    {
        tip = string.Empty;
        var disposableDropList = curEnvironmentBag.DisposableDropList;
        var repeatableDropList = curEnvironmentBag.RepeatableDropList;

        // 当一次性探索列表还有剩余
        if (!disposableDropList.IsEmpty)
        {
            // 掉落卡牌
            var droppedCards = disposableDropList.RandomDrop();
            if (droppedCards == null || droppedCards.Count == 0)
            {
                tip = "什么也没有得到";
                return;
            }


            AddCardsWithTween(droppedCards, envWindow.FrontCard.position, false);

            // 探索完成后让环境生态开始更新
            if (disposableDropList.IsEmpty)
                repeatableDropList.StartUpdating();

            // 探索度变化
            EventManager.Instance.TriggerEvent(EventType.ChangeDiscoveryDegree, (curEnvironmentBag.DiscoveryDegree, curEnvironmentBag.ExploreCompleted));
        }
        // 如果还可以重复探索
        else if (!repeatableDropList.IsEmpty)
        {
            var droppedCards = repeatableDropList.RandomDrop();
            if (droppedCards == null || droppedCards.Count == 0)
            {
                tip = "什么也没有得到";
                return;
            }

            AddCardsWithTween(droppedCards, envWindow.FrontCard.position, false);
        }
    }

    // 移动到目标场景
    public void Move(PlaceEnum targetPlace)
    {
        //拿到原先场景是哪个
        PlaceEnum lastPlace = curEnvironmentBag.PlaceData.placeType;

        foreach (var (place, bag) in environmentBags)
        {
            bag.gameObject.SetActive(place == targetPlace);
        }

        SoundManager.Instance.PlayPlaceMusic(environmentBags[targetPlace]);

        curEnvironmentBag = environmentBags[targetPlace];
        //从切换后的场景单次探索列表中拿出必定回到原先场景的牌，加入当前场景背包
        var door = curEnvironmentBag.DisposableDropList.CertainDrop($"通往{ParsePlaceEnum(lastPlace)}的门");
        if (door != null)
        {
            AddCard(door[0], false);
            door[0].Slot.RefreshCurrentDisplay();
        }

        EventManager.Instance.TriggerEvent(EventType.Move, curEnvironmentBag);
    }

    private string ParsePlaceEnum(PlaceEnum place)
    {
        return place switch
        {
            PlaceEnum.PowerCabin => "动力舱",
            PlaceEnum.Cockpit => "驾驶室",
            PlaceEnum.LifeSupportCabin => "维生舱",
            PlaceEnum.CoralCoast => "珊瑚礁海域",
            _ => null,
        };
    }
}