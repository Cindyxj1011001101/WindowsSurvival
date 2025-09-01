using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public interface IUpdate
{
    public void Update();
}

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
public class FreshnessComponent : CardComponent, IUpdate
{
    public int freshness;
    public int maxFreshness;

    public float updateRate = 1.0f;

    [JsonIgnore] public UnityAction onRotton;

    public FreshnessComponent() { }

    public FreshnessComponent(int maxFreshness)
    {
        freshness = this.maxFreshness = maxFreshness;
    }

    public void Update()
    {
        if (freshness <= 0) return;

        // 随时间自动减少新鲜度
        freshness -= (int)(TimeManager.Instance.SettleInterval * updateRate);
        freshness = Mathf.Max(freshness, 0);

        if (freshness <= 0)
        {
            BelongedCard.ShowTip($"{BelongedCard.CardName}腐烂了");
            freshness = 0;
            BelongedCard.DestroyThis();
            onRotton?.Invoke();
            return;
        }

        BelongedCard.RefreshSlot();
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
public class GrowthComponent : CardComponent, IUpdate
{
    public int growth;
    public int maxGrowth;

    public float updateRate = 1.0f;

    [JsonIgnore] public UnityAction onGrownUp;

    public GrowthComponent() { }

    public GrowthComponent(int maxGrowth)
    {
        this.maxGrowth = maxGrowth;
        growth = 0;
    }

    public void Update()
    {
        if (growth >= maxGrowth) return;

        // 随时间自动增加生长度
        growth += (int)(TimeManager.Instance.SettleInterval * updateRate);
        growth = Mathf.Min(growth, maxGrowth);

        if (growth >= maxGrowth)
        {
            growth = maxGrowth;
            BelongedCard.DestroyThis();
            onGrownUp?.Invoke();
            return;
        }

        BelongedCard.RefreshSlot();
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
public class ProgressComponent : CardComponent, IUpdate
{
    public int progress;
    public int maxProgress;

    public float updateRate = 1.0f;

    [JsonIgnore] public UnityAction onProgressFull;

    public ProgressComponent() { }

    public ProgressComponent(int maxProgress)
    {
        this.maxProgress = maxProgress;
        progress = 0;
    }

    public void Update()
    {
        if (progress >= maxProgress) return;

        // 随时间自动增加产物进度
        progress += (int)(TimeManager.Instance.SettleInterval * updateRate);
        progress = Mathf.Min(progress, maxProgress);

        if (progress >= maxProgress)
        {
            progress = maxProgress;
            BelongedCard.DestroyThis();
            onProgressFull?.Invoke();
            return;
        }

        BelongedCard.RefreshSlot();
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

    public EquipmentComponent() { }

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
    public List<ToolType> toolTypes = new();

    public ToolComponent() { }

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

    public DurabilityComponent() { }

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

    public bool display = true; // 是否显示内容物
    public bool allowAdd = true; // 是否允许添加内容物
    public bool allowRemove = true; // 是否允许移除内容物

    public InnerContentsComponent() { }

    public InnerContentsComponent(int slotCount)
    {
        bag.AddSlot(slotCount);
        bag.SetComponent(this);
    }

    public void Init()
    {
        bag.SetComponent(this);
        bag.Init();
    }

    public void Clear() => bag.Clear();

    public int GetTotalCountByCardId(string cardId) => bag.GetTotalCountByCardId(cardId);

    public int DestroyCardsByCardId(string cardId, int count) => bag.DestroyCardsByCardId(cardId, count);

    public bool CanQuickInteract(Card card)
    {
        return card.Moveable && card.Bag != bag && bag.CanAddCard(card, out _);
    }

    public void PauseUpdating()
    {
        ForEachCard(c => c.PauseUpdating());
    }

    public void ContinueUpdating()
    {
        ForEachCard(c => c.ContinueUpdating());
    }

    public void ForEachCard(UnityAction<Card> action)
    {
        foreach (var slot in bag.Slots)
        {
            foreach (var card in slot.Cards)
            {
                action?.Invoke(card);
            }
        }
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

    public FoodPropertyComponent() { }

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

    public FlammableComponent() { }

    public FlammableComponent(int fuelValue)
    {
        this.fuelValue = fuelValue;
    }
}
#endregion

#region 燃料存储组件
public class FuelStorageComponent : CardComponent
{
    public int fuel; // 燃料值
    public int maxFuel; // 最大燃料值
    public bool isFiring; // 是否正在燃烧

    public FuelStorageComponent() { }

    public FuelStorageComponent(int maxFuel)
    {
        fuel = 0;
        this.maxFuel = maxFuel;
        isFiring = false;
    }

    public void AddFuel(int delta)
    {
        fuel += delta;
        fuel = Mathf.Clamp(fuel, 0, maxFuel);
        BelongedCard.RefreshSlot();
    }

    public void SetIsFiring(bool firing)
    {
        isFiring = firing;
        BelongedCard.RefreshSlot();
    }

    public bool CanQuickInteract(Card card)
    {
        return card.TryGetComponent<FlammableComponent>(out var burnableComponent) && fuel < maxFuel;
    }

    public void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        for (int i = 0; i < count; i++)
        {
            if (fuel >= maxFuel) break;
            var card = slot.PeekCard();
            card.TryGetComponent<FlammableComponent>(out var burnableComponent);
            card.DestroyThis();
            AddFuel(burnableComponent.fuelValue);
        }
    }
}
#endregion

#region 通道组件
public class PassageComponent : CardComponent
{
    public PlaceEnum targetPlace;
    public int time;
    public string audioClip;

    public PassageComponent() { }

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

    public ConstructionComponent() { }

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

    public CookComponent() { }

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

#region 温度组件
public class TemperatureComponent : CardComponent
{
    public float temperature;
    public float maxTemperature;

    public TemperatureComponent() { }

    public TemperatureComponent(float temperature, float maxTemperature)
    {
        this.temperature = temperature;
        this.maxTemperature = maxTemperature;
    }

    public void AddTemperature(float delta)
    {
        temperature += delta;
        temperature = Mathf.Clamp(temperature, 0, maxTemperature);
        BelongedCard.RefreshSlot();
    }
}
#endregion

#region 状态机组件
public class CardState
{
    public string name; // 状态名称
    public string imagePath; // 图片路径
    public bool isAnim; // 是否为动画
    public bool needElectricity; // 是否需要电力
    public bool isConsumingElectricity; // 是否正在消耗电力

    public CardState() { }

    public CardState(string name, string imagePath, bool isAnim = false, bool needElectricity = false, bool isConsumingElectricity = false)
    {
        this.name = name;
        this.imagePath = imagePath;
        this.isAnim = isAnim;
        this.needElectricity = needElectricity;
        this.isConsumingElectricity = isConsumingElectricity;
    }
}

public class StateMachineComponent : CardComponent
{
    public string currentStateName;
    public Dictionary<string, CardState> stateDict = new();

    [JsonIgnore]
    public CardState CurrentState => stateDict[currentStateName];

    public StateMachineComponent() { }

    public StateMachineComponent(string initialStateName, List<CardState> states)
    {
        currentStateName = initialStateName;
        foreach (var state in states)
        {
            stateDict.Add(state.name, state);
        }
    }

    public void ChangeState(string newStateName)
    {
        if (!stateDict.ContainsKey(newStateName)) return;
        currentStateName = newStateName;
        BelongedCard.RefreshSlot();
    }
}
#endregion

#region 氧气存储组件
public class OxygenStorageComponent : CardComponent
{
    public float oxygen; // 氧气值
    public float maxOxygen; // 最大氧气值
    public OxygenStorageComponent() { }
    public OxygenStorageComponent(float maxOxygen)
    {
        oxygen = 0;
        this.maxOxygen = maxOxygen;
    }
    public void AddOxygen(float delta)
    {
        oxygen += delta;
        oxygen = Mathf.Clamp(oxygen, 0, maxOxygen);
        BelongedCard.RefreshSlot();
    }
}
#endregion

#region 植物生长组件
public class PlantGrowthComponent : CardComponent, IUpdate
{
    public float growthRate; // 生长速率
    public float growthProgress; // 生长进度
    public int deadProgress; // 死亡进度
    public float minConfortTempreture; // 最低舒适温度
    public float maxConfortTempreture; // 最高舒适温度
    public float minGrowTempture; // 最低生长温度
    public float maxGrowTempture; // 最高生长温度
    public float minLiveTempture; // 最低存活温度
    public float maxLiveTempture; // 最高存活温度
    public string deadCardId; // 死亡后变成的卡牌ID 
    public List<PressureLevel> pressureList=new List<PressureLevel>();
    public bool StopGrow=false; 
    
    [JsonIgnore] public UnityAction onDead;

    public PlantGrowthComponent(float growthRate, float minConfortTempreture, float maxConfortTempreture, float minGrowTempture, float maxGrowTempture, float minLiveTempture, float maxLiveTempture, string deadCardId, List<PressureLevel> pressureList)
    { 
        this.growthRate = growthRate;
        this.minConfortTempreture = minConfortTempreture;
        this.maxConfortTempreture = maxConfortTempreture;
        this.minGrowTempture = minGrowTempture;
        this.maxGrowTempture = maxGrowTempture;
        this.minLiveTempture = minLiveTempture;
        this.maxLiveTempture = maxLiveTempture;
        this.deadCardId = deadCardId;
        this.pressureList = pressureList;
        growthProgress = 0;
        deadProgress = 5; // 初始死亡进度
    }

    public void Update()
    {
        if (deadProgress <= 0) return;
        if(StopGrow)return;
        var bag = BelongedCard.Bag as EnvironmentBag;
        PressureLevel curPressureLevel = bag.PressureLevel;
        
        bag.StateDict.TryGetValue(EnvironmentStateEnum.RoomTemperature, out var t);
        float curTempture = 25;
        if (t == null)
        {
            Debug.LogWarning("当前没有环境温度信息，使用默认环境温度25度");
           
        }
        else
        {
            curTempture= t.CurValue;
        }

        if (!pressureList.Contains(curPressureLevel)) return;


        if (curTempture <= maxConfortTempreture && curTempture > minConfortTempreture)
        {
            growthProgress += growthRate * 1.2f; // 舒适区生长加快
        }
        else if (curTempture <= maxGrowTempture && curTempture > minGrowTempture)
        {
            growthProgress += growthRate * 1f;
        }
        else if (curTempture <= maxLiveTempture && curTempture > minLiveTempture)
        {
            //不生长
        }
        else
        {
            // 死亡进度增加
            deadProgress--;
        }

        if (deadProgress <= 0)
        {
            BelongedCard.ShowTip($"{BelongedCard.CardName}死亡了");
            deadProgress = 5;
            BelongedCard.DestroyThis();
            onDead?.Invoke();
            return;
        }

        BelongedCard.RefreshSlot();
    }
}
#endregion

#region 计时器组件
public class TimerComponent : CardComponent
{
    public float time;
    public float maxTime;

    public string tipText;

    public TimerComponent() { }

    public TimerComponent(float maxTime)
    {
        this.time = this.maxTime = maxTime;
    }

    public TimerComponent(float time, float maxTime)
    {
        this.time = time;
        this.maxTime = maxTime;
    }

    public void SetTime(float time)
    {
        this.time = time;
        BelongedCard.RefreshSlot();
    }

    public void Reset()
    {
        this.time = 0f;
        BelongedCard.RefreshSlot();
    }
}
#endregion
