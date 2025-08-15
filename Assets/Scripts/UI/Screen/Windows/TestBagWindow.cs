//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public abstract class TestBagWindow : WindowBase
//{
//    [SerializeField] protected GameObject slotPrefab; // 格子预制体
//    [SerializeField] protected Transform slotLayout; // 格子布局
//    [SerializeField] protected HoverableButton organizeButton; // 整理背包按钮

//    [SerializeField] protected List<CardSlot> slots;

//    protected Bag bag;

//    public void RefreshCurrentDisplay()
//    {
//        if (bag != null) DisplayBag(bag);
//    }

//    public void DisplayBag(Bag bag)
//    {
//        Clear();

//        this.bag = bag;

//        slots = new();
//        for (int i = 0; i < bag.Slots.Count; i++)
//        {
//            AddSlot().Init(bag.Slots[i]);
//        }

//        if (organizeButton != null)
//        {
//            organizeButton.onClick.RemoveAllListeners();
//            organizeButton.onClick.AddListener(() =>
//            {
//                if (bag.CompactCards())
//                    SoundManager.Instance.PlaySound("整理", true);
//                else
//                    SoundManager.Instance.PlaySound("低沉泡泡音", true, 1.3f);

//                RefreshCurrentDisplay();
                
//            });
//        }
//    }

//    /// <summary>
//    /// 添加指定数量的格子
//    /// </summary>
//    /// <param name="amount"></param>
//    public CardSlot AddSlot()
//    {
//        GameObject slotObj = Instantiate(slotPrefab, slotLayout.transform);
//        CardSlot slot = slotObj.GetComponent<CardSlot>();
//        slots.Add(slot);

//        // 添加格子后更新容器高度
//        MonoUtility.UpdateLayoutSize(slotLayout.GetComponent<ILayoutGroup>());

//        return slot;
//    }

//    public virtual void Clear()
//    {
//        bag = null;
//        MonoUtility.DestroyAllChildren(slotLayout.GetComponent<RectTransform>());
//    }
//}