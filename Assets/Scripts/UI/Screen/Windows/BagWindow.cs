using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BagWindow : WindowBase
{
    [SerializeField] protected GameObject slotPrefab; // 格子预制体
    [SerializeField] protected RectTransform slotLayout; // 格子布局
    [SerializeField] protected HoverableButton organizeButton; // 整理背包按钮

    [SerializeField] protected List<CardSlot> slots;

    public Bag Bag {  get; protected set; }

    public virtual void RefreshBagDisplay()
    {
        if (Bag != null) DisplayBag(Bag);
    }

    public virtual void DisplayBag(Bag bag)
    {
        ClearBag();

        Bag = bag;
        bag.SetBagWindow(this);

        slots = new();
        for (int i = 0; i < bag.Slots.Count; i++)
        {
            AddSlot().Init(bag.Slots[i]);
        }

        if (organizeButton != null)
        {
            organizeButton.onClick.RemoveAllListeners();
            organizeButton.onClick.AddListener(() =>
            {
                if (bag.CompactCards())
                    SoundManager.Instance.PlaySound("整理", true);
                else
                    SoundManager.Instance.PlaySound("低沉泡泡音", true, 1.3f);

                RefreshBagDisplay();

            });
        }
    }

    /// <summary>
    /// 添加指定数量的格子
    /// </summary>
    /// <param name="amount"></param>
    public CardSlot AddSlot()
    {
        GameObject slotObj = Instantiate(slotPrefab, slotLayout.transform);
        CardSlot slot = slotObj.GetComponent<CardSlot>();
        slots.Add(slot);

        // 添加格子后更新容器高度
        MonoUtility.UpdateLayoutSize(slotLayout.GetComponent<ILayoutGroup>());

        return slot;
    }

    public void RemoveSlot(CardSlot slot)
    {
        slots.Remove(slot);
        Destroy(slot.gameObject);

        MonoUtility.UpdateLayoutSize(slotLayout.GetComponent<ILayoutGroup>());
    }

    public virtual void ClearBag()
    {
        Bag?.SetBagWindow(null);
        Bag = null;
        MonoUtility.DestroyAllChildren(slotLayout);
    }
}