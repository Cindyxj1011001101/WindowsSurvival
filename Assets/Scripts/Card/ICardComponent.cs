using UnityEngine.Events;

/// <summary>
/// 组件接口
/// </summary>
public interface ICardComponent
{

}

// 新鲜度组件
public class FreshnessComponent : ICardComponent
{
    public int freshness;
    public int maxFreshness;

    public float updateRate = 1.0f;

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
            onFreshnessChanged?.Invoke(freshness);
        }
    }
}

// 生长度组件
public class GrowthComponent : ICardComponent
{
    public int growth;
    public int maxGrowth;

    public float updateRate = 1.0f;

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
            onGrowthChanged?.Invoke(growth);
        }
    }
}

// 产物进度组件
public class ProgressComponent : ICardComponent
{
    public int progress;
    public int maxProgress;
    public int productNum;
    public int maxProductNum; // 最大生产数量

    public float updateRate = 1.0f;

    public UnityAction<int> onProgressChanged;
    public UnityAction<int> onProductNumChanged;

    public ProgressComponent(int maxProgress, int maxProductNum,
        UnityAction<int> onProgressChanged, UnityAction<int> onProductNumChanged)
    {
        this.maxProgress = maxProgress;
        progress = 0;
        this.maxProductNum = maxProductNum;
        productNum = 0;
        this.onProgressChanged = onProgressChanged;
        this.onProductNumChanged = onProductNumChanged;
    }

    public void Update(int deltaTime)
    {
        if (productNum >= maxProductNum) return;
    
        // 随时间自动增加产物进度
        progress += (int)(deltaTime * updateRate);
        // 增加到maxProgress后，产物+1
        if (progress >= maxProgress)
        {
            progress -= maxProgress;
            productNum++;
            onProductNumChanged?.Invoke(productNum);
        }
        onProgressChanged?.Invoke(progress);
    }
}

public enum EquipmentType
{
    Head = 0,
    Body = 1,
    Back = 2,
    Leg = 3,
}

// 装备组件
public class EquipmentComponent : ICardComponent
{
    public EquipmentType equipmentType;

    public EquipmentComponent(EquipmentType equipmentType)
    {
        this.equipmentType = equipmentType;
    }
}

// 地点组件
public class PlaceComponent : ICardComponent
{
    public PlaceEnum placeType;

    public PlaceComponent(PlaceEnum placeType)
    {
        this.placeType = placeType;
    }
}