using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class ElectricalAppliance : IComparable<ElectricalAppliance>
{
    public string key;
    public float powerConsumptionRate;

    public ElectricalAppliance() { }

    public ElectricalAppliance(string key, float powerConsumptionRate)
    {
        this.key = key;
        this.powerConsumptionRate = powerConsumptionRate;
    }

    public int CompareTo(ElectricalAppliance other)
    {
        if (this == null && other == null) return 0;
        if (this == null) return 1;
        if (other == null) return -1;

        if (powerConsumptionRate != other.powerConsumptionRate)
            return powerConsumptionRate.CompareTo(other.powerConsumptionRate);

        return string.Compare(key, other.key, StringComparison.Ordinal);
    }
}

/// <summary>
/// 电力管理器
/// </summary>
public class ElectricPowerManager : IManager
{
    public static ElectricPowerManager Instance { get; private set; } = new();

    public State Power { get; private set; } = new();

    public SortedSet<ElectricalAppliance> SortedConnectedAppliances { get; private set; } = new();

    private Dictionary<string, (UnityAction powerOn, UnityAction powerOff)> powerOnOffActionsLookup = new();

    private Dictionary<string, ElectricalAppliance> connectedApplianceLookup = new();

    public void Init()
    {
        var data = GameDataManager.Instance.ElectricPowerData;
        if (!data.init)
        {
            Power = new(UnityEngine.Random.Range(30, 45), 50, higherIsBetter: true);
            SortedConnectedAppliances = new();
        }
        else
        {
            Power = data.power;
            SortedConnectedAppliances = data.connectedAppliances;
        }

        // 建立对照表
        foreach (var ea in SortedConnectedAppliances)
        {
            if (connectedApplianceLookup.ContainsKey(ea.key)) continue;
            connectedApplianceLookup.Add(ea.key, ea);
        }

        // 回合结算监听
        UpdateManager.Instance.PowerUpdate.AddListener(Update);
        EventManager.Instance.AddListener(EventType.UpdateBegin, OnUpdateBegin);

        // 监听行星磁暴事件
        EventManager.Instance.AddListener<GameEvent>(EventType.GameEventBegin, OnMagneticStormBegin);
        EventManager.Instance.AddListener<GameEvent>(EventType.GameEventEnd, OnMagneticStormEnd);
    }

    public void Reset()
    {
        Power = new();
        connectedApplianceLookup = new();
        powerOnOffActionsLookup.Clear();
        connectedApplianceLookup.Clear();
        UpdateManager.Instance.PowerUpdate.RemoveListener(Update);
        EventManager.Instance.RemoveListener(EventType.UpdateBegin, OnUpdateBegin);
        EventManager.Instance.RemoveListener<GameEvent>(EventType.GameEventBegin, OnMagneticStormBegin);
        EventManager.Instance.RemoveListener<GameEvent>(EventType.GameEventEnd, OnMagneticStormEnd);
    }

    private void OnMagneticStormBegin(GameEvent gameEvent)
    {
        if (gameEvent.GetType() != typeof(MagneticStorm)) return;

        // 断开所有连接的电器
        AutoDisconnectPower(true);

        RefreshElectricPower();
    }

    private void OnMagneticStormEnd(GameEvent gameEvent)
    {
        if (gameEvent.GetType() != typeof(MagneticStorm)) return;

        RefreshElectricPower();
    }

    /// <summary>
    /// 注册接电断电事件
    /// </summary>
    /// <param name="key"></param>
    /// <param name="powerOn"></param>
    /// <param name="powerOff"></param>
    public void RegisterPowerOnOffActions(string key, UnityAction powerOn, UnityAction powerOff)
    {
        if (powerOnOffActionsLookup.ContainsKey(key)) return;

        powerOnOffActionsLookup.Add(key, (powerOn, powerOff));
    }

    /// <summary>
    /// 接电
    /// </summary>
    /// <param name="key"></param>
    /// <param name="powerConsumptionRate"></param>
    public void ConnectPower(string key, float powerConsumptionRate)
    {
        if (IsAlreadyConnected(key)) return;

        // 加入对照表和已连接的电器列表
        var newApp = new ElectricalAppliance(key, powerConsumptionRate);
        connectedApplianceLookup.Add(key, newApp);
        SortedConnectedAppliances.Add(newApp);

        // 改变电力变化率
        ChangePowerConsumptionRate(-powerConsumptionRate);

        // 执行接电行为
        if (powerOnOffActionsLookup.ContainsKey(key))
        {
            powerOnOffActionsLookup[key].powerOn?.Invoke();
        }
    }

    /// <summary>
    /// 能否接电
    /// </summary>
    /// <param name="powerConsumptionRate"></param>
    public bool CanConnectPower(float powerConsumptionRate, out string reason)
    {
        reason = string.Empty;
        if (GameEventManager.Instance.IsEventOngoing<MagneticStorm>())
        {
            reason = "受磁暴影响，无法使用";
            return false;
        }

        if (Power.CurValue + Power.ChangeRate - powerConsumptionRate < 0)
        {
            reason = "电力供应不足";
            return false;
        }

        return true;
    }

    /// <summary>
    /// 是否已经接电
    /// </summary>
    /// <returns></returns>
    public bool IsAlreadyConnected(string key)
    {
        return connectedApplianceLookup.ContainsKey(key);
    }

    /// <summary>
    /// 断电
    /// </summary>
    /// <param name="key"></param>
    public void DisconnectPower(string key)
    {
        if (!connectedApplianceLookup.TryGetValue(key, out var value)) return;

        connectedApplianceLookup.Remove(key);
        SortedConnectedAppliances.Remove(value);

        // 改变电力变化率
        ChangePowerConsumptionRate(value.powerConsumptionRate);

        if (powerOnOffActionsLookup.ContainsKey(key))
        {
            powerOnOffActionsLookup[key].powerOff?.Invoke();
        }
    }

    /// <summary>
    /// 自动断电
    /// </summary>
    private void AutoDisconnectPower(bool disconnectAll)
    {
        while (SortedConnectedAppliances.Count > 0 &&
               (disconnectAll || Power.CurValue + Power.ChangeRate < 0))
        {
            UnityEngine.Debug.Log(SortedConnectedAppliances.Max.key + "断电了");
            DisconnectPower(SortedConnectedAppliances.Max.key);
        }
    }

    /// <summary>
    /// 改变电力(消耗或产生)
    /// </summary>
    /// <param name="delta"></param>
    public void ChangePower(float delta)
    {
        Power.AddValue(delta);

        // 若电力不足，根据耗电量从低到高断掉部分电器
        AutoDisconnectPower(false);

        RefreshElectricPower();
    }

    private void RefreshElectricPower()
    {
        var env = GameManager.Instance.CurEnvironmentBag;
        EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, new RefreshEnvironmentStateArgs(env.PlaceData.placeType, EnvironmentStateEnum.Electricity)
        {
            stateValue = Power
        });
    }

    /// <summary>
    /// 改变电力消耗率
    /// </summary>
    /// <param name="delta"></param>
    private void ChangePowerConsumptionRate(float delta)
    {
        Power.AddChangeRate(delta);

        RefreshElectricPower();
    }

    private float powerChangeRateSnapshot;

    private void OnUpdateBegin()
    {
        powerChangeRateSnapshot = Power.ChangeRate;
    }

    private void Update()
    {
        ChangePower(powerChangeRateSnapshot);
    }
}