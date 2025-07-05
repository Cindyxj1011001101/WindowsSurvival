using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    private static ButtonManager instance;
    public static ButtonManager Instance => instance;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void Init<T>(GameObject Panel) where T : PanelBase
    {
        string panelName = typeof(T).Name;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name == panelName)
                {
                    Panel.GetComponent<T>().SetStartButton(transform.GetChild(i).gameObject);
                    return;
                }
            }
    }
}
