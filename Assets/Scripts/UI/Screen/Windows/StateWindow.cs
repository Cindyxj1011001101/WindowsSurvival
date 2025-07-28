using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class StateWindow : WindowBase
{
    [SerializeField] private Transform stateLayout;

    [SerializeField] private GridLayoutGroup buffLayout;

    private Dictionary<PlayerStateEnum, UIStateSlider> stateSliders = new();

    protected override void Awake()
    {
        base.Awake();
        for (int i = 1; i < stateLayout.childCount; i++)
        {
            var child = stateLayout.GetChild(i);
            PlayerStateEnum stateType = (PlayerStateEnum)Enum.Parse(typeof(PlayerStateEnum), child.name);
            stateSliders.Add(stateType, child.GetComponentInChildren<UIStateSlider>());
        }
        EventManager.Instance.AddListener<PlayerStateEnum>(EventType.RefreshPlayerState, RefreshState);
    }

    public void OnDestroy()
    {
        EventManager.Instance.RemoveListener<PlayerStateEnum>(EventType.RefreshPlayerState, RefreshState);
    }

    //初始化显示数据
    protected override void Init()
    {
        foreach (PlayerStateEnum stateEnum in Enum.GetValues(typeof(PlayerStateEnum)))
        {
            if (!StateManager.Instance.PlayerStateDict.ContainsKey(stateEnum)) continue;
            RefreshState(stateEnum);
        }
        // 刷新布局大小
        MonoUtility.UpdateLayoutSize(stateLayout.GetComponent<ILayoutGroup>());
    }

    //更新显示数据
    public void RefreshState(PlayerStateEnum stateEnum)
    {
        PlayerState state = StateManager.Instance.PlayerStateDict[stateEnum];
        stateSliders[stateEnum].SetValue(state.CurValue, state.MaxValue);
    }
}