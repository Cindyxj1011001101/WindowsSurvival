using System.Collections.Generic;
using UnityEngine;

public class OxygenMask : Card
{
    public bool isEquipped;//是否已装备
    public OxygenMask()
    {
        cardName = "氧气面罩";
        cardDesc = "用于水下探索的简易氧气面罩，让你能看清水下世界。";
        cardImage = Resources.Load<Sprite>("CardImage/氧气面罩");
        cardType = CardType.Equipment;
        isEquipped = false;
        maxStackNum = 1;
        moveable = true;
        weight = 1.1f;
        events = new List<Event>();
        tags = new List<CardTag>();
        tags.Add(CardTag.Head);
        events.Add(new Event("装备", "装备氧气面罩", Event_Equip, () => Judge_Equip()));
        events.Add(new Event("卸下", "卸下氧气面罩", Event_UnEquip, () => Judge_UnEquip()));
    }

    public void Event_Equip()
    {
        if (!isEquipped)
        {
            isEquipped = true;
            EquipmentManager.Instance.EquipCard(this);
            StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, 30);
        }
    }

    public bool Judge_Equip()
    {
        return !isEquipped;
    }

    public void Event_UnEquip()
    {
        if (isEquipped)
        {
            isEquipped = false;
            EquipmentManager.Instance.UnequipCard(this);
            StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, -30);
        }
    }

    public bool Judge_UnEquip()
    {
        return isEquipped;
    }
}