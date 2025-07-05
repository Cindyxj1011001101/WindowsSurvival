using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        UIManager.Instance.ShowPanel<StatePanel>(PanelBase.ShowMode.Fade, onFinished: () =>
        {
            Debug.Log("显示面板动画完成");
        });
    }
}
