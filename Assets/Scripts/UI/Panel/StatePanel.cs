using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StatePanel : PanelBase
{
    [SerializeField]
    private Button startButton;
    [SerializeField]
    private Button exitButton;

    public void Awake()
    {
        ButtonManager.Instance.Init<StatePanel>(this.GameObject());
    }

    public override void SetStartButton(GameObject  button)
    {
        startButton=button.GetComponent<Button>();
    }
    protected override void Init()
    {
        startButton.onClick.AddListener(() =>
        {
            Debug.Log("打开");
            UIManager.Instance.ShowPanel<StatePanel>();
        });
        // 退出按钮点击后隐藏自己
        exitButton.onClick.AddListener(() =>
        {
            Debug.Log("关闭");
            // 注意：所有面板的显隐请通过UIManager.Instance.Show/HidePanel<>()来实现
            UIManager.Instance.HidePanel<StatePanel>();
        });
    }
}
