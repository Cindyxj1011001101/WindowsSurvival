/// <summary>
/// 小型气穴
/// </summary>
public class SmallAirFilledCave : Card
{
    private OxygenStorageComponent oxygenStorage;

    private SmallAirFilledCave()
    {
        Events = new()
        {
            new Event("获取氧气", "消耗矿石释氧机的氧气储存，补充自身氧气", oxygenStorage.Event_GetOxygen, oxygenStorage.Judge_GetOxygen)
        };
    }

    public override void Awake()
    {
        base.Awake();

        if (!TryGetComponent(out oxygenStorage))
        {
            oxygenStorage = new OxygenStorageComponent(200);
            AddComponent(oxygenStorage);
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        // 每回合补充2点氧气储量
        oxygenStorage.AddValue(2);
    }
}
