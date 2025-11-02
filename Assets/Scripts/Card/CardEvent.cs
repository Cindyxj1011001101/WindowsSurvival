using System;
using System.Collections.Generic;

/// <summary>
/// 卡牌事件
/// </summary>
public class CardEvent
{
    public string name;
    public string description;
    public string hint;
    public OutStringAction action;
    public OutStringAction<bool> condition;
    public Func<int> getTimeEffect;
    public Func<Dictionary<PlayerStateEnum, float>> getPlayerEffects;
    public Func<Dictionary<EnvironmentStateEnum, float>> getEnvEffects;

    public string Description => string.IsNullOrEmpty(hint) ? description : hint;

    public CardEvent(string name, string description, OutStringAction action, OutStringAction<bool> condition,
        Func<int> getTimeEffect = null, Func<Dictionary<PlayerStateEnum, float>> getPlayerEffects = null, Func<Dictionary<EnvironmentStateEnum, float>> getEnvEffects = null)
    {
        this.name = name;
        this.description = description;
        this.action = action;
        this.condition = condition;
        this.getTimeEffect = getTimeEffect;
        this.getPlayerEffects = getPlayerEffects;
        this.getEnvEffects = getEnvEffects;
    }

    public void Inovke(out string tip)
    {
        tip = string.Empty;
        action?.Invoke(out tip);
    }

    public bool Judge()
    {
        hint = string.Empty;
        if (condition == null || condition.Invoke(out hint))
        {
            return true;
        }

        return false;
    }

    public int GetTimeEffect()
    {
        if (getTimeEffect == null) return 0;
        return getTimeEffect.Invoke();
    }

    public Dictionary<PlayerStateEnum, float> GetPlayerEffects()
    {
        if (getPlayerEffects == null) return new();
        return getPlayerEffects.Invoke();
    }

    public Dictionary<EnvironmentStateEnum, float> GetEnvEffects()
    {
        if (getEnvEffects == null) return new();
        return getEnvEffects.Invoke();
    }
}

public delegate T OutStringAction<T>(out string s);
public delegate void OutStringAction(out string s);
public delegate void OutStringFunc<T>(out string s, T arg);