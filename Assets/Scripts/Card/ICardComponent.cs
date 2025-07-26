using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
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

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"新鲜度: {freshness}/{maxFreshness}\t");
        sb.Append($"更新速率: {updateRate}");
        return sb.ToString();
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

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"生长度: {growth}/{maxGrowth}\t");
        sb.Append($"更新速率: {updateRate}");
        return sb.ToString();
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

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"产物进度: {progress}/{maxProgress}\t");
        sb.Append($"更新速率: {updateRate}");
        return sb.ToString();
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

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"装备类型: {equipmentType}\t");
        sb.Append($"是否装备: {isEquipped}");
        return sb.ToString();
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
    public List<ToolType> toolTypes;

    public ToolComponent(params ToolType[] toolTypes)
    {
        this.toolTypes = new List<ToolType>(toolTypes);
    }

    public ToolComponent(List<ToolType> toolTypes)
    {
        this.toolTypes = toolTypes;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("工具类型: \t");
        foreach (var type in toolTypes)
        {
            sb.Append($"- {type}\t");
        }
        return sb.ToString();
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

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"耐久度: {durability}/{maxDurability}");
        return sb.ToString();
    }
}
#endregion

#region 内容物组件
public class InnerContentComponent : ICardComponent
{
    //public List<Card> innerContents;
    public List<List<Card>> innerContents = new();

    public int slotCount;

    [JsonIgnore]
    public Func<Card, bool> canAddContent;

    public InnerContentComponent(int slotCount)
    {
        this.slotCount = slotCount;
        innerContents = new();
        for (int i = 0; i < slotCount; i++)
        {
            innerContents.Add(new List<Card>());
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"内容物槽位数: {slotCount}\t");
        sb.Append("内容物: \n");
        for (int i = 0; i < innerContents.Count; i++)
        {
            sb.Append($"槽位 {i}: ");
            if (innerContents[i].Count == 0)
            {
                sb.Append("空");
            }
            else
            {
                foreach (var card in innerContents[i])
                {
                    sb.Append($"{card.CardId} ");
                }
            }
            sb.Append("\n");
        }
        return sb.ToString();
    }
}
#endregion