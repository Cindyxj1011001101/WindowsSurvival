using System.Collections.Generic;

public class PlayerBagData:VersionMigrator
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
    public override IVersionMigrator ReadJSON(string FilePath,string FileName)
    {
        return JsonManager.LoadData<PlayerBagData>(FilePath, FileName);
    }
}