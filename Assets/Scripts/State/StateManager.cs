using System.Collections.Generic;
using ScritableObject;
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
    public int curValue;
    public int MaxValue;
    public StateEnum stateEnum;
    public State(int value,int maxValue,StateEnum state)
    {
        curValue=value;
        MaxValue=maxValue;
        stateEnum = state;
    }
}
public class StateManager:MonoBehaviour
{
    public Dictionary<StateEnum, State>StateDict=new Dictionary<StateEnum, State>();

    
    public void Awake()
    {
        Init();
    }
    public void Start()
{
    EventManager.Instance.AddListener<ChangeStateArgs>(EventType.ChangeState, OnChangeState);
}
public void OnDestroy()
{
    EventManager.Instance.RemoveListener<ChangeStateArgs>(EventType.ChangeState, OnChangeState);
}
    public void Init()
    {
        StateDict.Add(StateEnum.Oxygen, new State(InitPlayerStateData.Instance.Oxygen,100,StateEnum.Oxygen));
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
}