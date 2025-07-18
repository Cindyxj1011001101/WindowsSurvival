using Newtonsoft.Json;
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

    [JsonIgnore]
    public UnityAction<int> onFreshnessChanged;

    public FreshnessComponent(int maxFreshness, UnityAction<int> onFreshnessChanged)
    {
        freshness = this.maxFreshness = maxFreshness;
        this.onFreshnessChanged = onFreshnessChanged;
    }

    public void Update(int deltaTime)
    {
        // 随时间自动减少新鲜度
        freshness -= (int)(deltaTime * updateRate);
        if (freshness <= 0)
        {
            freshness = 0;
        }
        onFreshnessChanged?.Invoke(freshness);
    }
}
#endregion

#region 生长度组件
public class GrowthComponent : ICardComponent
{
    public int growth;
    public int maxGrowth;

    public float updateRate = 1.0f;

    [JsonIgnore]
    public UnityAction<int> onGrowthChanged;

    public GrowthComponent(int maxGrowth, UnityAction<int> onGrowthChanged)
    {
        this.maxGrowth = maxGrowth;
        growth = 0;
        this.onGrowthChanged = onGrowthChanged;
    }

    public void Update(int deltaTime)
    {
        // 随时间自动增加生长度
        growth += (int)(deltaTime * updateRate);

        if (growth >= maxGrowth)
        {
            growth = maxGrowth;
        }
        onGrowthChanged?.Invoke(growth);
    }
}
#endregion

#region 产物进度组件
public class ProgressComponent : ICardComponent
{
    public int progress;
    public int maxProgress;

    public float updateRate = 1.0f;

    [JsonIgnore]
    public UnityAction onProgressFull;

    public ProgressComponent(int maxProgress, UnityAction onProgressFull)
    {
        this.maxProgress = maxProgress;
        progress = 0;
        this.onProgressFull = onProgressFull;
    }

    public void Update(int deltaTime)
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

#region 地点组件
public class PlaceComponent : ICardComponent
{
    public PlaceEnum placeType;

    public PlaceComponent(PlaceEnum placeType)
    {
        this.placeType = placeType;
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
    public int curDurability;
    public int maxDurability;

    [JsonIgnore]
    public UnityAction<int> onDurabilityChanged;

    public DurabilityComponent(int maxDurability, UnityAction<int> onDurabilityChanged)
    {
        curDurability = this.maxDurability = maxDurability;
        this.onDurabilityChanged = onDurabilityChanged;
    }

    public void Use()
    {
        curDurability--;
        if (curDurability < 0)
        {
            curDurability = 0;
        }
        onDurabilityChanged?.Invoke(curDurability);
    }
}
#endregion