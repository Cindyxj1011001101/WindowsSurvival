using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

public class StateWindow : WindowBase
{
    //订阅状态变化监听
    public Slider[] Sliders;
    protected override void Awake()
    {
        base.Awake();
        GameObject Container = GetComponentInChildren<GridLayoutGroup>().gameObject;
        Sliders = new Slider[Container.transform.childCount];
        for (int i = 0; i < Container.transform.childCount; i++)
        {
            Sliders[i] = Container.transform.GetChild(i).gameObject.GetComponentInChildren<Slider>();
        }
    }

    protected override void Start()
    {
        EventManager.Instance.AddListener<StateEnum>(EventType.RefreshState, RefreshState);
        base.Start();
    }

    public void OnDestroy()
    {
        EventManager.Instance.RemoveListener<StateEnum>(EventType.RefreshState, RefreshState);
    }

    //更新显示数据
    public void RefreshState(StateEnum stateEnum)
    {
        State state = StateManager.Instance.StateDict[stateEnum];
        Debug.Log("当前状态:"+stateEnum+" 当前值:"+state.curValue);
        Sliders[(int)stateEnum].value = state.curValue / state.MaxValue;
    }

    //初始化显示数据
    protected override void Init()
    {
        RefreshState(StateEnum.Fullness);
        RefreshState(StateEnum.Health);
        RefreshState(StateEnum.Thirst);
        RefreshState(StateEnum.San);
    }
}