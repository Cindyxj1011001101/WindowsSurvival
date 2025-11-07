/// <summary>
/// 小型气穴
/// </summary>
public class SmallAirFilledCave : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("获取氧气", $"消耗{CardName}的氧气储存，补充麦麦的氧气", oxygenStorage.Event_GetOxygen, oxygenStorage.Judge_GetOxygen);
    }

    protected override void OnLateConstructor()
    {
        oxygenStorage = new OxygenStorageComponent(200);
        AddComponent(oxygenStorage);
    }
    
    protected override void OnUpdate()
    {
        base.OnUpdate();

        // 每回合补充2点氧气储量
        oxygenStorage.AddValue(2);
    }
}
