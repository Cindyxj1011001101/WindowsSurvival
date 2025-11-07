using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : IManager
{
    public static GameManager Instance { get; private set; } = new();

    private float addCardTransition = 0.4f;

    public PlayerBag PlayerBag { get; private set; }
    public Dictionary<PlaceEnum, EnvironmentBag> EnvironmentBags { get; private set; } = new();
    public EnvironmentBag CurEnvironmentBag { get; private set; }
    public EquipmentBag EquipmentBag { get; private set; }
    public Dictionary<PlaceEnum, PlaceData> PlaceDataDict { get; private set; } = new();

    public bool IsCurrentEnvironment(Bag bag) => bag is EnvironmentBag env && env == CurEnvironmentBag;

    #region 初始化
    public void Init()
    {
        // 加载所有地点信息
        if (PlaceDataDict.IsNullOrEmpty())
        {
            foreach (var placeData in Resources.LoadAll<PlaceData>("ScriptableObject/Place"))
            {
                PlaceDataDict.Add(placeData.placeType, placeData);
            }
        }

        // 玩家背包
        PlayerBag = GameDataManager.Instance.PlayerBagData;
        // 所有环境背包
        EnvironmentBags = GameDataManager.Instance.EnvironmentBagDataDict;
        // 当前环境背包
        CurEnvironmentBag = EnvironmentBags[GameDataManager.Instance.LastPlace];
        EquipmentBag = GameDataManager.Instance.EquipmentData;

        PlayerBag.Init();
        EquipmentBag.Init();
        foreach (var bag in EnvironmentBags.Values)
        {
            bag.Init();
        }

        // 将玩家实体加入当前地点
        CurEnvironmentBag.AddEntity(Player.Instance);
    }

    public void Reset()
    {
        PlayerBag = new();
        EquipmentBag = new();
        EnvironmentBags = new();
    }
    #endregion

    /// <summary>
    /// 切换地点
    /// </summary>
    /// <param name="targetEnv">目标地点</param>
    public void ChangeEnv(PlaceEnum targetEnv)
    {
        // 玩家实体从原地点移除
        CurEnvironmentBag.RemoveEntity(Player.Instance);

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

        // 切换地点
        CurEnvironmentBag = EnvironmentBags[targetEnv];
        // 玩家实体添加到新地点
        CurEnvironmentBag.AddEntity(Player.Instance);

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
        EventManager.Instance.TriggerEvent(EventType.ChangeCurrentEnvironment, CurEnvironmentBag);
    }

    #region AddCard
    /// <summary>
    /// 添加卡牌到指定背包
    /// </summary>
    /// <param name="card"></param>
    /// <param name="targetBag"></param>
    public void AddCard(Card card, Bag targetBag)
    {
        targetBag.AddCard(card);
        card.Init();
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
            },
            pauseTime: true);
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
            },
            pauseTime: true);
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

    public void AddCardsToTargetEnv(EnvironmentBag targetEnv, params Card[] cards)
    {
        AddCardsToTargetEnv(cards.ToList(), targetEnv);
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
}