using System;
using System.Collections.Generic;

public class EnvironmentBagData:VersionMigrator
{
    public bool firstInit;
    public List<SlotCards> slots = new();
    public string placeName;
    public bool hasCable;
    public PressureLevel pressureLevel;
    public DisposableDropList disposableDropList = new();
    public RepeatableDropList repeatableDropList = new();
    public Dictionary<EnvironmentStateEnum, EnvironmentState> stateDict = new();
    public PlaceData placeData;

    public EnvironmentBag GetDataFromLoad()
    {
        EnvironmentBag bag = new EnvironmentBag();
        bag.firstInit = firstInit;
        bag.slots = slots;
        bag.placeName = placeName;
        bag.hasCable = hasCable;
        bag.pressureLevel = pressureLevel;
        bag.disposableDropList = disposableDropList;
        bag.repeatableDropList = repeatableDropList;
        bag.stateDict = stateDict;
        bag.placeData = placeData;
        return bag;
    }
    public override IVersionMigrator ReadJSON(string FilePath, string FileName)
    {
        return JsonManager.LoadData<EnvironmentBagData>(FilePath, FileName);
    }
}
public class EnvironmentBagDictData:VersionMigrator
{ 
    public Dictionary<PlaceEnum, EnvironmentBagData> dict = new();
    public Dictionary<PlaceEnum, EnvironmentBag> GetDataFromLoad()
    {
        Dictionary<PlaceEnum, EnvironmentBag> bags = new();
        foreach (var item in dict)
        {
            bags.Add(item.Key, item.Value.GetDataFromLoad());
        }
        return bags;
    }

    public override IVersionMigrator ReadJSON(string FilePath, string FileName)
    {
        dict.Clear();
        foreach (PlaceEnum place in Enum.GetValues(typeof(PlaceEnum)))
        {
            dict.Add(place,new EnvironmentBagData().ReadJSON(FilePath, place.ToString() + "Bag") as EnvironmentBagData);
        }
        return this;
    }
}