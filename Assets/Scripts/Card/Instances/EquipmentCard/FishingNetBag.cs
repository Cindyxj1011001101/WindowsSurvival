
public class FishingNetBag : EquipmentCard
{
    private FishingNetBag()
    {
        Events = new()
        {
            new Event("装备", "装备塑料袋", Event_Equip, Judge_Equip),
            new Event("卸下", "卸下塑料袋", Event_UnEquip, Judge_UnEquip),
            new Event("切割", "切割塑料袋", Event_Cut, Judge_Cut)
        };
    }
    public override void OnEquipped()
    {
    }

    public override void OnUnEquipped()
    {
    }

    public void Event_Cut(out string tip)
    {
        tip = string.Empty;
        Use();
        GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut).Use();
        AddCard("韧性胶管", true);
        AddCards("纤维", 4, true);
        TimeManager.Instance.AddTime(15);
    }

    public bool Judge_Cut(out string hint)
    {
        hint = string.Empty;
        if (TryGetComponent<InnerContentsComponent>(out InnerContentsComponent component))
        {
            if (component.bag.SlotCount == component.bag.EmptySlotCount && GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) != null)
            {
                return true;
            }
        }
        return false;
    }
}