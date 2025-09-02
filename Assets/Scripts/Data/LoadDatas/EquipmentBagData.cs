using System.Collections.Generic;

public class EquipmentBagData:VersionMigrator
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
    public override IVersionMigrator ReadJSON(string FilePath,string FileName)
    {
        return JsonManager.LoadData<EquipmentBagData>(FilePath, FileName);
    }
}