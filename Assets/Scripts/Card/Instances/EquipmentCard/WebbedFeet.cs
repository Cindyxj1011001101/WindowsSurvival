
public class WebbedFeet : EquipmentCard
{
    public override void LateInit()
    {
        base.LateInit();
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, OnMove);
    }

    public override void DestroyThis()
    {
        base.DestroyThis();
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move, OnMove);
    }

    public void OnMove(EnvironmentBag bag)
    {
        if (!equipment.isEquipped) return;

        if (!bag.PlaceData.isInWater) return;

        Use(1, () => ShowTip($"{CardName}损坏了"));
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