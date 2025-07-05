using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestPanel : PanelBase
{
    [SerializeField]
    private Button startButton;
    [SerializeField]
    private Button exitButton;

    protected override void Init()
    {
        startButton.onClick.AddListener(() =>
        {
            Debug.Log("ÓÎÏ·¿ªÊ¼");
        });
        exitButton.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<TestPanel>();
        });
    }
}
