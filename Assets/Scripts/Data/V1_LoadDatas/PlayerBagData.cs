using System.Collections.Generic;

public class PlayerBagData
{
    public bool firstInit;
    public List<SlotCards> slots;

    public PlayerBag GetDataFromLoad()
    {
        PlayerBag bag = new PlayerBag();
        bag.firstInit = firstInit;
        bag.slots = slots;
        return bag;
    }
}