using System.Collections.Generic;
using UnityEngine;

//玩家状态
public enum PlayerStateEnum
{
    Health,
    Fullness,
    Thirst,
    San,
    Oxygen,
    Tired,
}

//环境状态
public enum EnvironmentStateEnum
{
    Oxygen,
    Electricity,
    Temperature,
    Height,
    hasCable,
    isIndoor,
    isInWater,
}

//玩家状态类
public class PlayerState
{
    public float curValue;
    public float MaxValue;
    public PlayerStateEnum stateEnum;
    public PlayerState(float value, float maxValue, PlayerStateEnum state)
    {
        curValue = value;
        MaxValue = maxValue;
        stateEnum = state;
    }
}

//环境状态类
public class EnvironmentState
{
    public float curValue;
    public float MaxValue;
}
public class StateManager : MonoBehaviour
{
    public Dictionary<PlayerStateEnum, PlayerState> PlayerStateDict = new Dictionary<PlayerStateEnum, PlayerState>();
    private static StateManager instance;
    public static StateManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<StateManager>();
                if (instance == null)
                {
                    GameObject managerObj = new GameObject("StateManager");
                    instance = managerObj.AddComponent<StateManager>();
                    DontDestroyOnLoad(managerObj);
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Init();
    }
    public void Start()
    {
        EventManager.Instance.AddListener<ChangeStateArgs>(EventType.ChangeState, OnChangeState);
        EventManager.Instance.AddListener(EventType.IntervalSettle, IntervalSettle);
    }
    public void OnDestroy()
    {
        EventManager.Instance.RemoveListener<ChangeStateArgs>(EventType.ChangeState, OnChangeState);
        EventManager.Instance.RemoveListener(EventType.IntervalSettle, IntervalSettle);
    }
    public void Init()
    {
        PlayerStateDict.Add(PlayerStateEnum.Health, new PlayerState(InitPlayerStateData.Instance.Health, 100, PlayerStateEnum.Health));
        PlayerStateDict.Add(PlayerStateEnum.Fullness, new PlayerState(InitPlayerStateData.Instance.Fullness, 100, PlayerStateEnum.Fullness));
        PlayerStateDict.Add(PlayerStateEnum.Thirst, new PlayerState(InitPlayerStateData.Instance.Thirst, 100, PlayerStateEnum.Thirst));
        PlayerStateDict.Add(PlayerStateEnum.San, new PlayerState(InitPlayerStateData.Instance.San, 100, PlayerStateEnum.San));
    }

    public void OnChangeState(ChangeStateArgs args)
    {
        if (PlayerStateDict.ContainsKey(args.state))
        {
            PlayerStateDict[args.state].curValue += args.value;
            if (PlayerStateDict[args.state].curValue >= 100) PlayerStateDict[args.state].curValue = 100;
            if (PlayerStateDict[args.state].curValue <= 0) PlayerStateDict[args.state].curValue = 0;
        }

        EventManager.Instance.TriggerEvent(EventType.RefreshState, args.state);
    }

    public void IntervalSettle()
    {
        OnChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, InitPlayerStateData.Instance.BasicFullnessChange));
        OnChangeState(new ChangeStateArgs(PlayerStateEnum.Health, InitPlayerStateData.Instance.BasicHealthChange));
        OnChangeState(new ChangeStateArgs(PlayerStateEnum.Thirst, InitPlayerStateData.Instance.BasicThirstChange));
        OnChangeState(new ChangeStateArgs(PlayerStateEnum.San, InitPlayerStateData.Instance.BasicSanChange));
    }
}