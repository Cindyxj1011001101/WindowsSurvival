
public class WebbedFeet : EquipmentCard
{
    protected override void Start()
    {
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, OnMove);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move, OnMove);
    }

    public void OnMove(EnvironmentBag bag)
    {
        if (!equipment.isEquipped) return;

        if (!bag.PlaceData.isInWater) return;

        Use();
    }

    public override void OnEquipped()
    {
        //前往水域环境时消耗时间减半，耐久减一
        GameManager.Instance.AddMoveToWaterExtraEffect("装备了脚蹼", -0.5f, null);
    }

    public override void OnUnEquipped()
    {
        //恢复
        GameManager.Instance.RemoveMoveToWaterExtraEffect("装备了脚蹼");
    }
}