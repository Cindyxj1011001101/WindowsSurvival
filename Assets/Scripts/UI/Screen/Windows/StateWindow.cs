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
        base.Start();
        EventManager.Instance.AddListener<PlayerStateEnum>(EventType.RefreshState, RefreshState);
    }

    public void OnDestroy()
    {
        EventManager.Instance.RemoveListener<PlayerStateEnum>(EventType.RefreshState, RefreshState);
    }

    //更新显示数据
    public void RefreshState(PlayerStateEnum stateEnum)
    {
        PlayerState state = StateManager.Instance.PlayerStateDict[stateEnum];
        Sliders[(int)stateEnum].value = state.curValue / state.MaxValue;
    }

    //初始化显示数据
    protected override void Init()
    {
        RefreshState(PlayerStateEnum.Fullness);
        RefreshState(PlayerStateEnum.Health);
        RefreshState(PlayerStateEnum.Thirst);
        RefreshState(PlayerStateEnum.San);
    }
}