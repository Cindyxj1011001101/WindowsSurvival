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

    private DangerLevelEnum curDangerLevel;
    private bool init;

    public Image arrow;

    public RectTransform ceil;
    public RectTransform floor;

    public Sprite[] arrowSprites;

    private int curChangeLavel;

    public HoverTipController tipController;

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

        // 显示变化率
        DisplayChangeRate(state.ChangeRate, state.MaxValue);

        // 根据状态的危险程度，给予提示
        PlayerStateDangerAlert(state.DangerLevel);
    }

    public void SetValue(EnvironmentState state)
    {
        SetValue(state.CurValue, state.MaxValue);

        DisplayChangeRate(state.ChangeRate, state.MaxValue);
    }

    private void PlayerStateDangerAlert(DangerLevelEnum dangerLevel)
    {
        if (init && curDangerLevel == dangerLevel) return;

        init = true;
        curDangerLevel = dangerLevel;

        button.transform.DOKill();
        button.transform.localScale = Vector3.one;
        switch (dangerLevel)
        {
            case DangerLevelEnum.High:
                IconSizeYoloTween(1.25f, .3f);
                break;
            case DangerLevelEnum.Low:
                IconSizeYoloTween(1.1f, .5f);
                break;
            case DangerLevelEnum.None:
                break;
        }
    }

    private void IconSizeYoloTween(float scaleSize, float duration)
    {
        button.transform.DOScale(scaleSize, duration)
            .SetLoops(-1, LoopType.Yoyo) // 无限循环，来回播放
            .SetEase(Ease.InOutSine);    // 设置缓动效果
    }

    private void DisplayChangeRate(float changeRate, float maxValue)
    {
        var value = changeRate / maxValue;
        if (value == 0)
        {
            arrow.rectTransform.DOKill();
            arrow.gameObject.SetActive(false);
            tipController.enabled = false;
            return;
        }

        // 悬浮显示
        tipController.enabled = true;
        string tip;

        if (displayPercentage)
            tip = $"{changeRate / maxValue * 100:0.0}%";
        else
            tip = changeRate.ToString();

        tip = (changeRate > 0 ? "+" : "") + tip + "/15min";

        tipController.SetTip(tip);


        // 箭头显示
        int level = CalcLevel(value);
        if (curChangeLavel == level) return;

        arrow.gameObject.SetActive(true);
        arrow.rectTransform.DOKill();

        arrow.sprite = arrowSprites[Mathf.Abs(level) - 1];
        arrow.transform.localEulerAngles = new Vector3(value > 0 ? 0 : 180, 0, 0);

        float duration = .35f;
        if (value > 0)
        {
            arrow.rectTransform.DOAnchorPos(ceil.anchoredPosition, duration).From(floor.anchoredPosition).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            arrow.rectTransform.DOAnchorPos(floor.anchoredPosition, duration).From(ceil.anchoredPosition).SetLoops(-1, LoopType.Yoyo);
        }
    }

    /// <summary>
    /// 计算变化率等级
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private int CalcLevel(float value)
    {
        int signal = value > 0 ? 1 : -1;
        var absValue = Mathf.Abs(value);
        if (absValue <= 0.05)
            return 1 * signal;
        else if (absValue <= 0.1)
            return 2 * signal;
        else
            return 3 * signal;
    }
}