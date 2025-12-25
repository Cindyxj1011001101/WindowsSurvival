using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : IManager
{
    public static GameManager Instance { get; private set; } = new();

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

        // 加载/初始化完成后播放当前地点的环境音乐/环境音，确保读档后环境声音正确恢复
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayEnvironmentMusic(CurEnvironmentBag);
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
    /// <param name="createReturnPassage">是否在目标地点创建回到原地点的通道卡（开发者面板跳转无需创建）</param>
    public void ChangeEnv(PlaceEnum targetEnv, bool createReturnPassage = true)
    {
        var lastEnv = CurEnvironmentBag;
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

        var env = CurEnvironmentBag;

        if (createReturnPassage)
        {
            // 从切换后的场景单次探索列表中拿出回到原先场景的牌，加入当前场景背包
            var passageCardId = $"从{env.PlaceName}到{lastEnv.PlaceName}";
            Card passageCard = env.FindCardOfId(passageCardId);
            // 先尝试从探索列表里面取出
            if (passageCard == null)
            {
                var droppedCards = env.DisposableDropList.CertainDrop(passageCardId);
                if (!droppedCards.IsNullOrEmpty())
                {
                    passageCard = droppedCards[0];
                }
                // 探索列表里面没有就直接创建
                passageCard ??= CardFactory.CreateCard(passageCardId);
                // 加入当前场景背包
                AddCard(passageCard, env);
            }

            // 玩家坐标设置在通道位置
            passageCard.TryGetComponent<CoordinateComponent>(out var coordinate);
            Player.Instance.MoveTo(coordinate.coordinate.Position);
        }

        // 播放新地点环境音（使用统一的播放入口）
        SoundManager.Instance.PlayEnvironmentMusic(CurEnvironmentBag);

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
    public Tween AddCardWithTween(Card card, Bag targetBag, Vector3 sourcePosition)
    {
        AddCard(card, targetBag);

        return AnimationManager.Instance.PlayAddCard(card, sourcePosition);
    }

    public Tween AddCardWithTween(Card card, bool toPlayerBag, Vector3 sourcePosition)
    {
        AddCard(card, toPlayerBag);

        return AnimationManager.Instance.PlayAddCard(card, sourcePosition);
    }

    public Tween AddCardsWithTween(List<Card> cards, bool toPlayerBag, Vector3 sourcePosition)
    {
        foreach (var card in cards)
        {
            AddCard(card, toPlayerBag);
        }

        return AnimationManager.Instance.PlayAddCards(cards.ToArray(), sourcePosition);
    }

    public Tween AddCardToTargetEnv(Card card, EnvironmentBag targetEnv)
    {
        return AddCardsToTargetEnv(new List<Card> { card }, targetEnv);
    }

    public Tween AddCardsToTargetEnv(List<Card> cards, EnvironmentBag targetEnv)
    {
        if (targetEnv == CurEnvironmentBag)
        {
            return AddCardsWithTween(cards, false, Vector3.up * 600);
        }

        foreach (var card in cards)
        {
            AddCard(card, targetEnv);
        }
        return null;
    }
    #endregion

    #region 装备
    /// <summary>
    /// 穿上装备
    /// </summary>
    /// <param name="equipment"></param>
    public void Equip(Card equipment, Vector3 sourcePosition)
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
        AddCardWithTween(equipment, EquipmentBag, sourcePosition); // transform 理论上不会为空
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