public class EquipmentWindow : BagWindow
{
    //private Dictionary<EquipmentType, EquipmentCardSlot> equipmentSlotDict;

    protected override void Init()
    {
        //InitBag(GameDataManager.Instance.EquipmentData);
        DisplayBag(GameDataManager.Instance.EquipmentData.GetDataFromLoad());
    }

    public override void DisplayBag(Bag bag)
    {
        //Clear();

        Bag = bag;
        bag.SetBagWindow(this);

        //slots = new();
        //for (int i = 0; i < bag.Slots.Count; i++)
        //{
        //    AddSlot().Init(bag.Slots[i]);
        //}

        //if (organizeButton != null)
        //{
        //    organizeButton.onClick.RemoveAllListeners();
        //    organizeButton.onClick.AddListener(() =>
        //    {
        //        if (bag.CompactCards())
        //            SoundManager.Instance.PlaySound("����", true);
        //        else
        //            SoundManager.Instance.PlaySound("�ͳ�������", true, 1.3f);

        //        RefreshDisplay();

        //    });
        //}

        for (int i = 0; i < bag.SlotCount; i++)
        {
            slots[i].Init(bag[i]);
        }
    }

    //protected override void InitBag(BagRuntimeData runtimeData)
    //{
    //    for (int i = 0; i < runtimeData.cardSlots.Count; i++)
    //    {
    //        slots[i].SetBag(this);
    //        slots[i].Init(runtimeData.cardSlots[i]);
    //    }
    //    equipmentSlotDict = new()
    //    {
    //        { EquipmentType.Head, slots[0] as EquipmentCardSlot},
    //        { EquipmentType.Body, slots[1] as EquipmentCardSlot},
    //        { EquipmentType.Back, slots[2] as EquipmentCardSlot},
    //        { EquipmentType.Leg, slots[3] as EquipmentCardSlot},
    //    };
    //}
}