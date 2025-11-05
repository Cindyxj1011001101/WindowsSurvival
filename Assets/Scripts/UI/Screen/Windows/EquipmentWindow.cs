public class EquipmentWindow : BagWindow
{
    protected override void Init()
    {
        DisplayBag(GameManager.Instance.EquipmentBag);
    }

    public override void DisplayBag(Bag bag)
    {
        Bag = bag;
        bag.SetBagWindow(this);

        for (int i = 0; i < bag.SlotCount; i++)
        {
            slots[i].Init(bag[i]);
        }
    }
}