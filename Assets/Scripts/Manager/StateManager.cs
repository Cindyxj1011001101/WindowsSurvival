using System.Collections.Generic;
using UnityEngine;

public enum StateEnum
{
    //氧气、健康值、饱食、水分、困倦度、san值
    Health,
    Fullness,
    Thirst,
    San
}
public class State
{
    public float curValue;
    public float MaxValue;
    public StateEnum stateEnum;
    public State(float value, float maxValue, StateEnum state)
    {
        curValue = value;
        MaxValue = maxValue;
        stateEnum = state;
    }
}
public class StateManager : MonoBehaviour
{
    public Dictionary<StateEnum, State> StateDict = new Dictionary<StateEnum, State>();
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
                    DontDestroyOnLoad(managerObj); // 跨场景保持实例
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        // 确保只有一个实例
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
        StateDict.Add(StateEnum.Health, new State(InitPlayerStateData.Instance.Health, 100, StateEnum.Health));
        StateDict.Add(StateEnum.Fullness, new State(InitPlayerStateData.Instance.Fullness, 100, StateEnum.Fullness));
        StateDict.Add(StateEnum.Thirst, new State(InitPlayerStateData.Instance.Thirst, 100, StateEnum.Thirst));
        StateDict.Add(StateEnum.San, new State(InitPlayerStateData.Instance.San, 100, StateEnum.San));
    }

    public void OnChangeState(ChangeStateArgs args)
    {
        if (StateDict.ContainsKey(args.state))
        {
            StateDict[args.state].curValue += args.value;
            if (StateDict[args.state].curValue >= 100) StateDict[args.state].curValue = 100;
            if (StateDict[args.state].curValue <= 0) StateDict[args.state].curValue = 0;
        }

        EventManager.Instance.TriggerEvent(EventType.RefreshState, args.state);
    }

    public void IntervalSettle()
    {
        OnChangeState(new ChangeStateArgs(StateEnum.Fullness, InitPlayerStateData.Instance.BasicFullnessChange));
        OnChangeState(new ChangeStateArgs(StateEnum.Health, InitPlayerStateData.Instance.BasicHealthChange));
        OnChangeState(new ChangeStateArgs(StateEnum.Thirst, InitPlayerStateData.Instance.BasicThirstChange));
        OnChangeState(new ChangeStateArgs(StateEnum.San, InitPlayerStateData.Instance.BasicSanChange));
    }
}