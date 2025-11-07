using System;
using System.Collections.Generic;

/// <summary>
/// 卡牌事件
/// </summary>
public class CardEvent
{
    private string name;
    private string description;
    private Func<string> getDescription;
    private string hint;
    private OutStringAction action;
    private OutStringFunc<bool> condition;
    private Func<int> getTimeChange;
    private Func<Dictionary<PlayerStateEnum, float>> getPlayerStateChanges;
    private Func<Dictionary<EnvironmentStateEnum, float>> getEnvStateChanges;

    public string Description
    {
        get
        {
            if (!string.IsNullOrEmpty(hint)) return hint;

            if (getDescription != null) return getDescription();

            return description;
        }
    }
    public string Name => name;

    public CardEvent(
        string name,
        string description,
        OutStringAction action,
        OutStringFunc<bool> condition,
        Func<int> getTimeChange = null,
        Func<Dictionary<PlayerStateEnum, float>> getPlayerStateChanges = null,
        Func<Dictionary<EnvironmentStateEnum, float>> getEnvStateChanges = null)
    {
        this.name = name;
        this.description = description;
        this.action = action;
        this.condition = condition;
        this.getTimeChange = getTimeChange;
        this.getPlayerStateChanges = getPlayerStateChanges;
        this.getEnvStateChanges = getEnvStateChanges;
    }

    public CardEvent(
        string name,
        Func<string> getDescription,
        OutStringAction action,
        OutStringFunc<bool> condition,
        Func<int> getTimeChange = null,
        Func<Dictionary<PlayerStateEnum, float>> getPlayerStateChanges = null,
        Func<Dictionary<EnvironmentStateEnum, float>> getEnvStateChanges = null) : this(name, string.Empty, action, condition, getTimeChange, getPlayerStateChanges, getEnvStateChanges)
    {
        this.getDescription = getDescription;
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

    public int GetTimeChange()
    {
        if (getTimeChange == null) return 0;
        return getTimeChange.Invoke();
    }

    public Dictionary<PlayerStateEnum, float> GetPlayerStateChanges()
    {
        if (getPlayerStateChanges == null) return new();
        return getPlayerStateChanges.Invoke();
    }

    public Dictionary<EnvironmentStateEnum, float> GetEnvStateChanges()
    {
        if (getEnvStateChanges == null) return new();
        return getEnvStateChanges.Invoke();
    }
}

public delegate T OutStringFunc<T>(out string s);
public delegate void OutStringAction(out string s);
public delegate void OutStringAction<T>(out string s, T arg);