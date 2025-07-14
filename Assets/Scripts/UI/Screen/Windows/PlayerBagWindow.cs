using System;
using UnityEngine.UI;

public class PlayerBagWindow : BagWindow
{
    private Text loadText; // 载重显示

    protected override void Awake()
    {
        base.Awake();
        loadText = transform.Find("TopBar/CurrentLoad").GetComponent<Text>();
        EventManager.Instance.AddListener/*<ChangeLoadArgs>*/(EventType.ChangeLoad, OnLoadChanged);
    }

    protected override void Init()
    {
        // 显示载重
        OnLoadChanged();
    }

    private void OnLoadChanged(/*ChangeLoadArgs args*/)
    {
        //DisplayBagLoad(args.currentLoad, args.maxLoad);
        DisplayBagLoad(StateManager.Instance.curLoad, StateManager.Instance.maxLoad);
    }

    private void DisplayBagLoad(float currentLoad, float maxLoad)
    {
        loadText.text = $"载重: {Math.Round(currentLoad, 1)} / {Math.Round(maxLoad, 1)}";
    }
    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener/*<ChangeLoadArgs>*/(EventType.ChangeLoad, OnLoadChanged);
    }
}