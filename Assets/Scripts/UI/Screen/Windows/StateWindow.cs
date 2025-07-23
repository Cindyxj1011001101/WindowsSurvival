using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class StateWindow : WindowBase
{
    [SerializeField] private Transform stateLayout;

    [SerializeField] private GridLayoutGroup buffLayout;

    private Dictionary<PlayerStateEnum, StateSlider> stateSliders = new();

    protected override void Awake()
    {
        base.Awake();
        for (int i = 1; i < stateLayout.childCount; i++)
        {
            var child = stateLayout.GetChild(i);
            PlayerStateEnum stateType = (PlayerStateEnum)Enum.Parse(typeof(PlayerStateEnum), child.name);
            stateSliders.Add(stateType, child.GetComponentInChildren<StateSlider>());
        }
        EventManager.Instance.AddListener<PlayerStateEnum>(EventType.RefreshPlayerState, RefreshState);
    }

    //初始化显示数据
    protected override void Init()
    {
        RefreshState(PlayerStateEnum.Fullness);
        RefreshState(PlayerStateEnum.Health);
        RefreshState(PlayerStateEnum.Thirst);
        RefreshState(PlayerStateEnum.San);
        RefreshState(PlayerStateEnum.Oxygen);
        RefreshState(PlayerStateEnum.Soberiety);
    }

    public void OnDestroy()
    {
        EventManager.Instance.RemoveListener<PlayerStateEnum>(EventType.RefreshPlayerState, RefreshState);
    }

    //更新显示数据
    public void RefreshState(PlayerStateEnum stateEnum)
    {
        PlayerState state = StateManager.Instance.PlayerStateDict[stateEnum];
        stateSliders[stateEnum].DisplayState(state.CurValue, state.MaxValue);
    }
}