using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 组件接口
/// </summary>
public abstract class CardComponent
{
    public Card BelongedCard { get; private set; }

    public void SetBelongedCard(Card card)
    {
        BelongedCard = card;
    }
}

#region 新鲜度组件
public class FreshnessComponent : CardComponent
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
        freshness = Mathf.Max(freshness, 0);

        BelongedCard.RefreshSlot();

        if (freshness <= 0)
        {
            BelongedCard.ShowTip($"{BelongedCard.CardName}腐烂了");
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
public class GrowthComponent : CardComponent
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
        growth = Mathf.Min(growth, maxGrowth);

        BelongedCard.RefreshSlot();

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
public class ProgressComponent : CardComponent
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
        progress = Mathf.Min(progress, maxProgress);

        BelongedCard.RefreshSlot();

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

public class EquipmentComponent : CardComponent
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
    Hammer,//锤击
}

public class ToolComponent : CardComponent
{
    public List<ToolType> toolTypes;

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
public class DurabilityComponent : CardComponent
{
    public int durability;
    public int maxDurability;

    public DurabilityComponent(int maxDurability)
    {
        durability = this.maxDurability = maxDurability;
    }

    public void Use(UnityAction onBroken)
    {
        if (durability <= 0) return;

        durability--;
        durability = Mathf.Max(durability, 0);

        BelongedCard.RefreshSlot();

        if (durability <= 0)
        {
            durability = 0;
            onBroken?.Invoke();
        }
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
public delegate bool CardFilterDelegate(Card card, out string s);

public class InnerContentsComponent : CardComponent
{
    public float weightLossRate = 1f; // 减重率

    public InnerBag bag = new();

    [JsonIgnore] public CardFilterDelegate contentFilter;
    [JsonIgnore] public UnityAction<Card> onAddCard;
    [JsonIgnore] public UnityAction<Card> onRemoveCard;

    public bool display; // 是否显示内容物
    public bool canAddOrRemove; // 是否可以添加或移除内容物

    public void Init()
    {
        bag.SetComponent(this);
        bag.Init();
    }

    public void Clear() => bag.Clear();

    public InnerContentsComponent(int slotCount)
    {
        bag.AddSlot(slotCount);
        bag.SetComponent(this);
        display = true; // 默认显示内容物
        canAddOrRemove = true; // 默认可以添加或移除内容物
    }

    public int GetTotalCountByCardId(string cardId) => bag.GetTotalCountByCardId(cardId);

    public int DestroyCardsByCardId(string cardId, int count) => bag.DestroyCardsByCardId(cardId, count);

    public bool CanQuickInteract(Card card)
    {
        return card.Moveable && card.Bag != bag && bag.CanAddCard(card, out _);
    }

    public void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        for (int i = 0; i < count; i++)
        {
            if (!bag.CanAddCard(slot.PeekCard(), out tip)) break;
            var toAdd = slot.RemoveCard();
            bag.AddCard(toAdd);
            toAdd.RefreshSlot();
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"内容物槽位数: {bag.SlotCount}\t");
        sb.Append("内容物: \n");
        for (int i = 0; i < bag.SlotCount; i++)
        {
            sb.Append($"槽位 {i}: ");
            if (bag[i].IsEmpty)
            {
                sb.Append("空");
            }
            else
            {
                foreach (var card in bag.Slots[i].Cards)
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

#region 食物属性
public class FoodPropertyComponent : CardComponent
{
    public Dictionary<FoodProperty, int> foodPropertyDict;

    public FoodPropertyComponent(Dictionary<FoodProperty, int> foodPropertyDict)
    {
        this.foodPropertyDict = foodPropertyDict;
    }
}
#endregion

#region 可燃烧组件
public class FlammableComponent : CardComponent
{
    public int fuelValue; // 燃料值

    public FlammableComponent(int fuelValue)
    {
        this.fuelValue = fuelValue;
    }
}
#endregion

#region 燃料存储组件
public class FuelContainerComponent : CardComponent
{
    public int fuel; // 燃料值
    public int maxFuel; // 最大燃料值
    public FuelContainerComponent(int maxFuel)
    {
        fuel = 0;
        this.maxFuel = maxFuel;
    }

    public void AddFuel(int delta)
    {
        fuel += delta;
        fuel = Mathf.Clamp(fuel, 0, maxFuel);
        BelongedCard.RefreshSlot();
    }
}
#endregion

#region 通道组件
public class PassageComponent : CardComponent
{
    public PlaceEnum targetPlace;
    public int time;
    public string audioClip;

    public PassageComponent(PlaceEnum targetPlace, int time, string audioClip)
    {
        this.targetPlace = targetPlace;
        this.time = time;
        this.audioClip = audioClip;
    }
}
#endregion

#region 建筑组件
public class ConstructionComponent : CardComponent
{
    public bool onlyInWater;
    public bool onlyOutWater;
    public bool onlyInDoor;
    public bool onlyOutDoor;
    public bool needCable;

    public bool canBeDemolished; // 能否被拆毁
    public string demolitionDebris; // 拆毁后产物ID

    public ConstructionComponent(bool onlyInWater, bool onlyOutWater, bool onlyInDoor, bool onlyOutDoor, bool needCable, bool canBeDemolished, string demolitionDebris)
    {
        this.onlyInWater = onlyInWater;
        this.onlyOutWater = onlyOutWater;
        this.onlyInDoor = onlyInDoor;
        this.onlyOutDoor = onlyOutDoor;
        this.needCable = needCable;
        this.canBeDemolished = canBeDemolished;
        this.demolitionDebris = demolitionDebris;
    }
}
#endregion

#region 烹饪组件
public class CookComponent : CardComponent
{
    public int totalCookTime;
    public int leftCookTime;
    public string outcomeCardId;

    public CookComponent(int totalCookTime, string outcomeCardId)
    {
        this.totalCookTime = leftCookTime = totalCookTime;
        this.outcomeCardId = outcomeCardId;
    }

    public void Update(int deltaTime, UnityAction<string> onCooked)
    {
        if (leftCookTime <= 0) return;

        leftCookTime -= deltaTime;
        leftCookTime = Mathf.Max(leftCookTime, 0);
        if (leftCookTime <= 0)
        {
            leftCookTime = 0;
            onCooked?.Invoke(outcomeCardId);
        }
    }
}
#endregion