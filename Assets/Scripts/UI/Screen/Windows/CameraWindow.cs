using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraWindow : WindowBase
{
    public Button sleepButton;
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Init()
    {
        sleepButton=transform.Find("Body/Sleep").GetComponent<Button>();
        sleepButton.onClick.AddListener(OnSleepButtonClick);
    }

    private void OnSleepButtonClick()
    {
        TimeManager.Instance.AddTime(240);
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Tired, -56));
    }


}
