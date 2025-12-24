using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class StateWindow : WindowBase
{
    [SerializeField] private Transform stateLayout;

    [SerializeField] private GridLayoutGroup buffLayout;

    [HideInInspector] public Dictionary<PlayerStateEnum, UIStateSlider> stateSliders = new();

    protected override void Awake()
    {
        base.Awake();
        for (int i = 1; i < stateLayout.childCount; i++)
        {
            var child = stateLayout.GetChild(i);
            PlayerStateEnum stateType = (PlayerStateEnum)Enum.Parse(typeof(PlayerStateEnum), child.name);
            stateSliders.Add(stateType, child.GetComponentInChildren<UIStateSlider>());
        }
        EventManager.Instance.AddListener<PlayerStateEnum>(EventType.RefreshPlayerState, RefreshStateValue);
    }

    public void OnDestroy()
    {
        EventManager.Instance.RemoveListener<PlayerStateEnum>(EventType.RefreshPlayerState, RefreshStateValue);
    }

    //初始化显示数据
    protected override void Init()
    {
        foreach (PlayerStateEnum stateEnum in Enum.GetValues(typeof(PlayerStateEnum)))
        {
            if (!StateManager.Instance.PlayerStateDict.ContainsKey(stateEnum)) continue;
            DisplayStateValue(stateEnum, false);
        }
        // 刷新布局大小
        MonoUtility.UpdateLayoutSize(stateLayout.GetComponent<ILayoutGroup>());
    }

    //更新显示数据
    private void DisplayStateValue(PlayerStateEnum stateEnum, bool playAnim)
    {
        stateSliders[stateEnum].SetValue(StateManager.Instance.PlayerStateDict[stateEnum], playAnim);
    }

    private void RefreshStateValue(PlayerStateEnum stateEnum)
    {
        DisplayStateValue(stateEnum, true);
    }
}