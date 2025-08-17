
public class WebbedFeet : EquipmentCard
{
    private WebbedFeet()
    {
        Events = new()
        {
            new Event("装备", "装备脚蹼", Event_Equip, Judge_Equip),
            new Event("卸下", "卸下脚蹼", Event_UnEquip, Judge_UnEquip)
        };
    }
    public override void OnEquipped()
    {
        //前往水域环境时消耗时间减半，耐久减一
    }

    public override void OnUnEquipped()
    {
        //恢复
    }
}