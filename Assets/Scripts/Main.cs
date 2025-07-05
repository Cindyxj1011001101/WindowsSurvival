using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.ShowPanel<TestPanel>(PanelBase.ShowMode.Animator, onFinished: () =>
        {
            Debug.Log("显示动画播放完毕");
        });
    }
}
