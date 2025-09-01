using System.Collections.Generic;

public class EquipmentBagData
{
    public bool firstInit;
    public List<SlotCards> slots;

    public EquipmentBag GetDataFromLoad()
    {
        EquipmentBag bag = new EquipmentBag();
        bag.firstInit = firstInit;
        bag.slots = slots;
        return bag;
    }
}