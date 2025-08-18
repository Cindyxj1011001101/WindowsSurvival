
public class HeavyOxygenCan : EquipmentCard
{
    private HeavyOxygenCan()
    {
        Events = new()
        {
            new Event("装备", "装备脚蹼", Event_Equip, Judge_Equip),
            new Event("卸下", "卸下脚蹼", Event_UnEquip, Judge_UnEquip)
        };
    }
    protected override void LateInit()
    {
        base.LateInit();
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move,OnMove);
    }

    public override void DestroyThis()
    {
        base.DestroyThis();
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move,OnMove);
    }

    public void OnMove(EnvironmentBag bag)
    {
        if (bag.PlaceData.isInWater)
        {
            Use();
            //LIANG-TODO:额外消耗50%移动时间

        }
    }

    public override void OnEquipped()
    {
        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, 800);
        //LIANG-TODO:额外消耗探索时间
    }

    public override void OnUnEquipped()
    {
        //移除在地点移动额外消耗20%时间，探索额外消耗20%时间
        StateManager.Instance.ChangePlayerExtraState(PlayerStateEnum.Oxygen, -800);
    }
}