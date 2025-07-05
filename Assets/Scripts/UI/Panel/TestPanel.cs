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

    public override void SetStartButton(GameObject button)
    {
        return;
    }

    protected override void Init()
    {
        startButton.onClick.AddListener(() =>
        {
            Debug.Log("��Ϸ��ʼ");
        });
        exitButton.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<TestPanel>();
        });
    }
}
