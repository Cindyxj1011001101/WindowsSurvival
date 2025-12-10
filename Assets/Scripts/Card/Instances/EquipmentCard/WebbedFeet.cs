/// <summary>
/// 脚蹼
/// </summary>
[CardId("脚蹼")]
public class WebbedFeet : EquipmentCard
{
    protected override void OnInit()
    {
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.ChangeCurrentEnvironment, OnChangeEnv);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.ChangeCurrentEnvironment, OnChangeEnv);
    }

    public void OnChangeEnv(EnvironmentBag bag)
    {
        if (!equipment.isEquipped) return;

        if (!bag.PlaceData.isInWater) return;

        Use();
    }

    public override void OnEquipped()
    {
        // 前往水域环境时消耗时间-30%
        MoveExploreManager.Instance.AddMoveToWaterExtraEffect("装备了脚蹼", -0.3f, null);
    }

    public override void OnUnEquipped()
    {
        MoveExploreManager.Instance.RemoveMoveToWaterExtraEffect("装备了脚蹼");
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (!equipment.isEquipped) return;

        // 装备时每回合消耗0.4耐久
        Use(.4f);
    }
}