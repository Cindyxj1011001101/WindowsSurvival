using UnityEngine;
using UnityEngine.UI;

public class BackpackWindow : WindowBase
{
    private Text loadText; // 载重显示

    protected override void Init()
    {
        loadText = transform.Find("TopBar/CurrentLoad").GetComponent<Text>();
        EventManager.Instance.AddListener<ChangeLoadArgs>(EventType.ChangeLoad, OnLoadChanged);
    }

    private void OnLoadChanged(ChangeLoadArgs args)
    {
        loadText.text = $"载重: {args.currentLoad} / {args.maxLoad}";
    }
}