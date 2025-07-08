using System.Collections.Generic;
using UnityEngine;

public abstract class BagBase : MonoBehaviour
{
    protected GameObject slotPrefab;
    protected Transform slotContainer;
    [Header("��ʼ��������")]
    [SerializeField]
    protected int initSlotCount = 9;

    protected List<CardSlot> slots = new();

    protected int UsedSlotsCount // ��ʹ�õĸ��ӵ�����
    {
        get
        {
            int count = 0;
            foreach (CardSlot slot in slots)
            {
                if (!slot.IsEmpty) count++;
            }
            return count;
        }
    }
    protected int SlotsCount => slots.Count; // ���и��ӵ�����
    protected bool IsBagFull => UsedSlotsCount == SlotsCount; // �����Ƿ�����

    protected virtual void Start()
    {
        slotPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/CardSlot");
        slotContainer = transform.Find("Viewport/Container");
        InitBag();
    }

    protected void InitBag()
    {
        slots = new();
        // ���Ӹ���
        AddSlot(initSlotCount);
    }

    protected void AddSlot(int amount)
    {
        for(int i = 0; i < amount; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotContainer);
            CardSlot slot = slotObj.GetComponent<CardSlot>();
            slots.Add(slot);
        }
    }

    public virtual bool CanAddCard(CardInstance card)
    {
        // �����п�λ��һ�������ӿ���
        if (!IsBagFull) return true;

        // ����û�п�λ
        // �ܷ�����ȡ���ڱ����е�ͬ�࿨���Ƿ�ﵽ�ѵ�����
        string cardName = card.CardData.cardName;
        var slots = GetSlotsContainingSimilarCard(cardName);
        foreach (CardSlot slot in slots)
        {
            if (slot.CanStack()) return true;
        }

        return false;
    }

    /// <summary>
    /// ��ȡ���з���ͬ�࿨�Ƶĸ���
    /// </summary>
    /// <param name="cardName">��������</param>
    /// <param name="ascending">true: ���նѵ�������������false: ��������</param>
    /// <returns></returns>
    protected List<CardSlot> GetSlotsContainingSimilarCard(string cardName, bool ascending = true)
    {
        List<CardSlot> result = new();
        foreach (CardSlot slot in slots)
        {
            if (slot.ContainsSimilarCard(cardName))
                result.Add(slot);
        }
        result.Sort((a, b) => ascending ? a.StackCount - b.StackCount : b.StackCount - a.StackCount);
        return result;
    }

    public virtual void AddCard(CardInstance card)
    {
        //if (!CanAddCard(card)) return;

        string cardName = card.CardData.cardName;

        // ���ȳ��Զѵ������п���
        foreach (var slot in GetSlotsContainingSimilarCard(cardName))
        {
            if (slot.CanStack())
            {
                slot.AddCard(card);
                return;
            }
        }

        // ����ŵ��µĿ�λ
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.AddCard(card);
                return;
            }
        }
    }

    public virtual void RemoveCard(CardInstance card)
    {
        string cardName = card.CardData.cardName;

        var slots = GetSlotsContainingSimilarCard(cardName, false);

        if (slots.Count == 0) return;

        // �ܵõ����slot˵������������һ���ƣ����Կ���ֱ�Ӵ������Ƴ�����
        slots[0].RemoveCard();
    }

    public virtual void Clear()
    {
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }
    }

    /// <summary>
    /// �������п��ƣ���ÿ�����ƾ�������ǰ�ƶ�
    /// </summary>
    public void CompactCards()
    {
        // ��¼��Ҫ�ƶ��Ŀ��ƺ����ǵ�ԭʼλ��
        List<(CardSlot slot, int index)> nonEmptySlots = new List<(CardSlot, int)>();

        // ��һ�α������ռ����зǿղ�λ��Ϣ
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty)
            {
                nonEmptySlots.Add((slots[i], i));
            }
        }

        // �ڶ��α�������ǰ��������λ
        int currentPosition = 0;
        foreach (var (slot, index) in nonEmptySlots)
        {
            // �����ǰ�����Ѿ�����ȷλ�ã�����
            if (currentPosition == index)
            {
                currentPosition++;
                continue;
            }

            // �ƶ����Ƶ���ǰλ��
            MoveCardToPosition(slot, currentPosition);
            currentPosition++;
        }
    }

    /// <summary>
    /// �����ƴ�һ����λ�ƶ�����һ����λ
    /// </summary>
    private void MoveCardToPosition(CardSlot sourceSlot, int targetIndex)
    {
        // ���Ŀ��λ�þ��ǵ�ǰλ�ã������κβ���
        int sourceIndex = slots.IndexOf(sourceSlot);
        if (sourceIndex == targetIndex) return;

        CardSlot targetSlot = slots[targetIndex];

        // ���Ŀ���λΪ�գ�ֱ���ƶ������ѵ�
        if (targetSlot.IsEmpty)
        {
            while (sourceSlot.StackCount > 0)
            {
                targetSlot.AddCard(sourceSlot.RemoveCard());
            }
        }
        // ���Ŀ���λ����ͬ�����ҿ��Զѵ�
        else if (targetSlot.ContainsSimilarCard(sourceSlot.Card.cardName) && targetSlot.CanStack())
        {
            // �����ܶ���ƶ����Ƶ�Ŀ���λ
            while (sourceSlot.StackCount > 0 && targetSlot.CanStack())
            {
                targetSlot.AddCard(sourceSlot.RemoveCard());
            }
        }
    }
}