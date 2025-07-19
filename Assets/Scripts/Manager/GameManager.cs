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

    private PlayerBag playerBag;
    private Dictionary<PlaceEnum, EnvironmentBag> environmentBags = new();
    private EnvironmentBag curEnvironmentBag;
    private EquipmentBag equipmentBag;

    public PlayerBag PlayerBag => playerBag;
    public Dictionary<PlaceEnum, EnvironmentBag> EnvironmentBags => environmentBags;
    public EnvironmentBag CurEnvironmentBag => curEnvironmentBag;
    public EquipmentBag EquipmentBag => equipmentBag;

    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
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
    }

    public void AddCard(Card card, bool toPlayerBag)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("抽卡", true);

        // 卡牌的属性开始随时间变化
        card.StartUpdating();
        if (toPlayerBag)
        {
            if (playerBag.CanAddCard(card))
                playerBag.AddCard(card);
            else
                curEnvironmentBag.AddCard(card);
        }
        else
        {
            curEnvironmentBag.AddCard(card);
        }
    }

    public Card AddCard(string cardId, bool toPlayerBag)
    {
        var card = CardFactory.CreateCard(cardId);
        AddCard(card, toPlayerBag);
        return card;
    }

    public void HandleExplore()
    {
        var disposableDropList = curEnvironmentBag.disposableDropList;
        var repeatableDropList = curEnvironmentBag.repeatableDropList;
        if (disposableDropList.IsEmpty && repeatableDropList.IsEmpty)
        {
            Debug.Log("探索完全");
            return;
        }

        float explorationTime = curEnvironmentBag.explorationTime;

        switch (curEnvironmentBag.PlaceData.placeType)
        {
            case PlaceEnum.PowerCabin:
                break;
            case PlaceEnum.Cockpit:
                break;
            case PlaceEnum.LifeSupportCabin:
                break;
            case PlaceEnum.CoralCoast:
                // 如果没有佩戴氧气面罩
                if (equipmentBag.FindCardOfName("氧气面罩") == null)
                {
                    // 探索时间+40%
                    explorationTime *= 1.4f;
                    // 健康值-4
                    StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -4));
                }
                break;
            default:
                break;
        }

        // 消耗时间
        TimeManager.Instance.AddTime((int)explorationTime);

        HandeleExploreDrop();
    }


    private void HandeleExploreDrop()
    {
        var disposableDropList = curEnvironmentBag.disposableDropList;
        var repeatableDropList = curEnvironmentBag.repeatableDropList;

        // 当一次性探索列表还有剩余
        if (!disposableDropList.IsEmpty)
        {
            // 掉落卡牌
            foreach (var card in disposableDropList.RandomDrop())
            {
                // 掉落到环境里
                AddCard(card, false);
            }

            // 探索完成后让环境生态开始更新
            if (disposableDropList.IsEmpty)
                repeatableDropList.StartUpdating();

            // 探索度变化
            EventManager.Instance.TriggerEvent(EventType.ChangeDiscoveryDegree, curEnvironmentBag.DiscoveryDegree);
        }
        // 如果还可以重复探索
        else if (!repeatableDropList.IsEmpty)
        {
            var droppedCards = repeatableDropList.RandomDrop();
            if (droppedCards == null || droppedCards.Count == 0)
            {
                Debug.Log("什么也没有捞到");
                return;
            }

            // 掉落卡牌
            foreach (var card in droppedCards)
            {
                // 掉落到环境里
                AddCard(card, false);
            }
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
        curEnvironmentBag = environmentBags[targetPlace];

        //从切换后的场景单次探索列表中拿出必定回到原先场景的牌，加入当前场景背包
        var door = curEnvironmentBag.disposableDropList.CertainDrop($"通往{ParsePlaceEnum(lastPlace)}的门");
        if (door != null)
            AddCard(door[0], false);

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