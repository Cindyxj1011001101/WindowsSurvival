using System.Collections.Generic;
using UnityEngine;
public enum PlaceEnum
{
    /// <summary>
    /// ������
    /// </summary>
    PowerCabin,
    /// <summary>
    /// ��ʻ��
    /// </summary>
    Cockpit,
    /// <summary>
    /// ά����
    /// </summary>
    LifeSupportCabin

}
public class EffectResolve : MonoBehaviour
{
    private static EffectResolve instance;
    public static EffectResolve Instance => instance;

    private PlayerBag playerBag;
    private Dictionary<PlaceEnum, EnvironmentBag> environmentBags = new();
    private EnvironmentBag curEnvironmentBag;

    public PlayerBag PlayerBag => playerBag;
    public Dictionary<PlaceEnum, EnvironmentBag> EnvironmentBags => environmentBags;
    public EnvironmentBag CurEnvironmentBag => curEnvironmentBag;

    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        // ��¼��ұ���
        playerBag = FindObjectOfType<PlayerBag>(true);
        // ��¼���л�������
        foreach (var bag in FindObjectsOfType<EnvironmentBag>(true))
        {
            environmentBags.Add(bag.PlaceData.placeType, bag);
        }
        // ��ǰ��������
        curEnvironmentBag = environmentBags[GameDataManager.Instance.LastPlace];
        Init();
    }

    //初始化SO数据
    public void Init()
    {
        EventTrigger[] eventTriggers = Resources.LoadAll<EventTrigger>("ScriptableObject/EventTrigger");
        if (eventTriggers != null && eventTriggers.Length > 0)
        {
            foreach (var trigger in eventTriggers)
            {
                trigger.Init();
            }
        }
    }

    //̽������
    public void ResolveExplore()
    {
        CardEvent cardEvent = curEnvironmentBag.CardEvent;
        foreach (var EventTrigger in cardEvent.eventList)
        {
            //�����̽���¼��������⴦��
            if (EventTrigger.GetType() == typeof(PlaceDropEvent))
            {
                EventTrigger.EventResolve();
            }
        }
    }

    //�ж��Ƿ������¼���������
    public bool ConditionEventJudge(CardEvent cardEvent)
    {
        if (cardEvent.GetType() == typeof(ConditionalCardEvent))
        {
            ConditionalCardEvent conditionCardEvent = cardEvent as ConditionalCardEvent;
            foreach (var conditionData in conditionCardEvent.ConditionCardList)
            {
                CardSlot slot = playerBag.TryGetCardByCondition(conditionData);
                if (slot == null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    //��������¼���������
    public void Resolve(CardEvent cardEvent)
    {
        if (cardEvent == null) return;

        if (!ConditionEventJudge(cardEvent)) return;
        //״̬����
        foreach (var EventTrigger in cardEvent.eventList)
        {
            if (EventTrigger.GetType() == typeof(ValueEvent)) EventTrigger.EventResolve();
        }
        //�����л�
        foreach (var EventTrigger in cardEvent.eventList)
        {
            if (EventTrigger.GetType() == typeof(MoveEvent)) EventTrigger.EventResolve();
        }
        //ʱ������
        TimeManager.Instance.AddTime(cardEvent.Time);
        //���俨Ƭ
        foreach (var EventTrigger in cardEvent.eventList)
        {
            if (EventTrigger.GetType() == typeof(DropEvent)) EventTrigger.EventResolve();
        }
    }

    //���俨�Ƽ��뱳��
    public void AddDropCard(Drop drop, bool ToPlayerBag)
    {
        for (int i = 0; i < drop.DropNum; i++)
        {
            if (drop.cardData == null) continue;

            CardInstance cardInstance = CardFactory.CreateCardIntance(drop.cardData);
            if (ToPlayerBag)
            {
                //�жϱ������������Ƿ�����
                if (playerBag.CanAddCard(cardInstance))
                {
                    playerBag.AddCard(cardInstance);
                }
                else
                {
                    curEnvironmentBag.AddCard(cardInstance);
                }
            }
            else
            {
                curEnvironmentBag.AddCard(cardInstance);
            }
        }

    }

    //�����ƶ�
    public void Move(PlaceEnum targetPlace)
    {
        foreach (var (place, bag) in environmentBags)
        {
            bag.gameObject.SetActive(place == targetPlace);
        }
        curEnvironmentBag = environmentBags[targetPlace];

        EventManager.Instance.TriggerEvent(EventType.Move, curEnvironmentBag);
    }
}