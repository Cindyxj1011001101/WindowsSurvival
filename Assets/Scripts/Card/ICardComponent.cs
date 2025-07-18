using UnityEngine.Events;

/// <summary>
/// 组件接口
/// </summary>
public interface ICardComponent
{

}

#region 新鲜度组件
public class FreshnessComponent : ICardComponent
{
    public int freshness;
    public int maxFreshness;

    public float updateRate = 1.0f;

    public FreshnessComponent(int maxFreshness)
    {
        freshness = this.maxFreshness = maxFreshness;
    }

    public void Update(int deltaTime, UnityAction onRotton)
    {
        if (freshness <= 0) return;

        // 随时间自动减少新鲜度
        freshness -= (int)(deltaTime * updateRate);
        if (freshness <= 0)
        {
            freshness = 0;
            onRotton?.Invoke();
        }
    }
}
#endregion

#region 生长度组件
public class GrowthComponent : ICardComponent
{
    public int growth;
    public int maxGrowth;

    public float updateRate = 1.0f;

    public GrowthComponent(int maxGrowth)
    {
        this.maxGrowth = maxGrowth;
        growth = 0;
    }

    public void Update(int deltaTime, UnityAction onGrownUp)
    {
        if (growth >= maxGrowth) return;

        // 随时间自动增加生长度
        growth += (int)(deltaTime * updateRate);

        if (growth >= maxGrowth)
        {
            growth = maxGrowth;
            onGrownUp?.Invoke();
        }
    }
}
#endregion

#region 产物进度组件
public class ProgressComponent : ICardComponent
{
    public int progress;
    public int maxProgress;

    public float updateRate = 1.0f;

    public ProgressComponent(int maxProgress)
    {
        this.maxProgress = maxProgress;
        progress = 0;
    }

    public void Update(int deltaTime, UnityAction onProgressFull)
    {
        if (progress >= maxProgress) return;

        // 随时间自动增加产物进度
        progress += (int)(deltaTime * updateRate);
        if (progress >= maxProgress)
        {
            progress = maxProgress;
            onProgressFull?.Invoke();
        }
    }
}
#endregion

#region 装备组件
public enum EquipmentType
{
    Head = 0,
    Body = 1,
    Back = 2,
    Leg = 3,
}

public class EquipmentComponent : ICardComponent
{
    public EquipmentType equipmentType;
    public bool isEquipped;

    public EquipmentComponent(EquipmentType equipmentType)
    {
        isEquipped = false;
        this.equipmentType = equipmentType;
    }
}
#endregion

#region 工具组件
public enum ToolType
{
    Cut,//切割
    Dig,//挖掘
}

public class ToolComponent : ICardComponent
{
    public ToolType toolType;

    public ToolComponent(ToolType toolType)
    {
        this.toolType = toolType;
    }
}
#endregion

#region 耐久度组件
public class DurabilityComponent : ICardComponent
{
    public int durability;
    public int maxDurability;

    public DurabilityComponent(int maxDurability)
    {
        durability = this.maxDurability = maxDurability;
    }
}
#endregion