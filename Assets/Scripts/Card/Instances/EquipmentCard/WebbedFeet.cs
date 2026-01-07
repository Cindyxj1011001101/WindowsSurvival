/// <summary>
/// 脚蹼
/// </summary>
[CardId("脚蹼")]
public class WebbedFeet : EquipmentCard
{
    private const float TIME_DECREAST_RATE = 0.3f;

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

    public override string GetEquipDesc()
    {
        return $"麦麦前往水域地点消耗的时间减少{ColorManager.ColorizePercent(TIME_DECREAST_RATE, ColorManager.Green, "0")}，且在水域地点中移动消耗的时间减少{ColorManager.ColorizePercent(TIME_DECREAST_RATE, ColorManager.Green, "0")}";
    }

    public override void OnEquipped()
    {
        // 前往水域环境时消耗时间-30%
        MoveExploreManager.Instance.AddMoveInWaterExtraEffect("装备了脚蹼", -TIME_DECREAST_RATE, null);
    }

    public override void OnUnEquipped()
    {
        MoveExploreManager.Instance.RemoveMoveInWaterExtraEffect("装备了脚蹼");
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (!equipment.isEquipped) return;

        // 装备时每回合消耗0.4耐久
        Use(.4f);
    }
}