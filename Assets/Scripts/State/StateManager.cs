using System.Collections.Generic;
using UnityEngine;

public enum StateEnum
{
    //氧气、健康值、饱食、水分、困倦度、san值
    Oxygen,
    Health,
    Fullness,
    Thirst,
    Tired,
    San
}
public class State
{
    public float curValue;
    public float MaxValue;
    public StateEnum stateEnum;
    public State(float value,float maxValue,StateEnum state)
    {
        curValue=value;
        MaxValue=maxValue;
        stateEnum = state;
    }
}
public class StateManager:MonoBehaviour
{
    public Dictionary<StateEnum, State>StateDict=new Dictionary<StateEnum, State>();
    private static StateManager instance;
    public static StateManager Instance => instance;
    
    public void Awake()
    {
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
        StateDict.Add(StateEnum.Health, new State(InitPlayerStateData.Instance.Health,100,StateEnum.Health));
        StateDict.Add(StateEnum.Fullness, new State(InitPlayerStateData.Instance.Fullness,100,StateEnum.Fullness));
        StateDict.Add(StateEnum.Thirst, new State(InitPlayerStateData.Instance.Thirst,100,StateEnum.Thirst));
        StateDict.Add(StateEnum.Tired, new State(InitPlayerStateData.Instance.Tired,100,StateEnum.Tired));
        StateDict.Add(StateEnum.San, new State(InitPlayerStateData.Instance.San,100,StateEnum.San));
    }
    
    public void OnChangeState(ChangeStateArgs args)
    {
        if (StateDict.ContainsKey(args.state))
        {
            StateDict[args.state].curValue += args.value;
            StateDict[args.state].curValue=StateDict[args.state].curValue>=StateDict[args.state].MaxValue?StateDict[args.state].MaxValue:StateDict[args.state].curValue;
            StateDict[args.state].curValue=StateDict[args.state].curValue<=0?0:StateDict[args.state].curValue;
        }
    }

    public void IntervalSettle()
    {
        StateDict[StateEnum.Fullness].curValue += InitPlayerStateData.Instance.BasicFullnessChange;
        StateDict[StateEnum.Thirst].curValue += InitPlayerStateData.Instance.BasicThirstChange;
        StateDict[StateEnum.Health].curValue +=  InitPlayerStateData.Instance.BasicHealthChange;
        StateDict[StateEnum.Tired].curValue += InitPlayerStateData.Instance.BasicTiredChange;
        StateDict[StateEnum.San].curValue +=  InitPlayerStateData.Instance.BasicSanChange;
    }
}