public class EquipmentCardInstance : CardInstance
{
    public bool IsEquipped; //是否装备

    public void Equip()
    {
        IsEquipped = true;
    }

    public void Unequip()
    {
        IsEquipped = false; 
    }
}