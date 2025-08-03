using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIStateSlider : MonoBehaviour
{
    public Image icon;
    public Text stateNameText;
    public Text valueText;
    public Slider slider;

    public HoverableButton button;

    public bool displayPercentage;

    public void SetStateName(string name)
    {
        stateNameText.text = name;
    }

    public virtual void SetValue(float value, float maxValue)
    {
        slider.value = value / maxValue;
        if (displayPercentage)
            valueText.text = $"{value * 100 / maxValue: 0.0}%";
        else
            valueText.text = $"{(int)value}/{maxValue}";
    }

    public void SetValue(PlayerState state)
    {
        SetValue(state.CurValue, state.MaxValue);

        // 根据状态的危险程度，给予提示
        PlayerStateDangerAlert(state.DangerLevel);
    }

    public void SetValue(EnvironmentState state)
    {
        SetValue(state.CurValue, state.MaxValue);
    }

    private void PlayerStateDangerAlert(DangerLevelEnum dangerLevel)
    {
        switch (dangerLevel)
        {
            case DangerLevelEnum.High:
                IconSizeYoloTween(1.25f, .3f);
                break;
            case DangerLevelEnum.Low:
                IconSizeYoloTween(1.1f, .5f);
                break;
            case DangerLevelEnum.None:
                button.transform.DOKill();
                button.transform.localScale = Vector3.one;
                break;
        }
    }

    private void IconSizeYoloTween(float scaleSize, float duration)
    {
        button.transform.DOKill();
        button.transform.DOScale(scaleSize, duration)
            .SetLoops(-1, LoopType.Yoyo) // 无限循环，来回播放
            .SetEase(Ease.InOutSine);    // 设置缓动效果
    }
}