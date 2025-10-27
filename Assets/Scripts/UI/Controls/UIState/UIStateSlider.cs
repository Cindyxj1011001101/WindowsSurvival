using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIStateSlider : MonoBehaviour
{
    public Image icon;
    public Text stateNameText;
    public Text valueText;
    public Slider slider;

    public Image arrow;
    public RectTransform ceil;
    public RectTransform floor;
    public Sprite[] arrowSprites;

    public HoverableButton button;
    public HoverTipController tipController;

    public bool displayPercentage;          // 是否以百分比形式显示数值

    private DangerLevelEnum curDangerLevel; // 当前危险等级
    private bool init;                      // 是否已初始化

    private int curChangeLavel;             // 当前变化率等级

    public Color fillColor = ColorManager.White;

    public float value => slider.value;

    private void OnDisable()
    {
        init = false;
        if (button != null) button.transform.DOKill();
        if (arrow != null) arrow.transform.DOKill();
        if (icon != null) icon.transform.DOKill();
        fillColor = ColorManager.White;
    }

    public void SetStateName(string name)
    {
        stateNameText.text = name;
    }

    public virtual void SetValue(float value, float maxValue)
    {
        if (slider.fillRect.TryGetComponent<Image>(out var fill))
        {
            fill.color = fillColor;
        }

        if (slider.handleRect != null)
        {
            var handle = slider.handleRect.GetComponentInChildren<Image>();
            if (handle != null)
            {
                handle.color = fillColor;
            }
        }

        slider.value = value / maxValue;

        if (valueText == null) return;

        if (displayPercentage)
            valueText.text = $"{value * 100 / maxValue: 0.0}%";
        else
            valueText.text = $"{(int)value}/{maxValue}";
    }

    public void SetValue(State state)
    {
        SetValue(state.CurValue, state.MaxValue);

        // 显示变化率
        DisplayChangeRate(state.ChangeRate, state.CurValue, state.MaxValue, state.HigherIsBetter, state.LowerIsBetter);

        // 根据状态的危险程度，给予提示
        PlayerStateDangerAlert(state.DangerLevel);
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

    private void DisplayChangeRate(float changeRate, float value, float maxValue,
        bool higherIsBetter, bool lowerIsBetter)
    {
        var percentage = changeRate / maxValue;
        if (percentage == 0)
        {
            arrow.rectTransform.DOKill();
            arrow.gameObject.SetActive(false);
            tipController.enabled = false;
            curChangeLavel = 0;
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
        var lastLevel = curChangeLavel;
        curChangeLavel = CalcLevel(percentage);

        // 如果当前值已经为0 且 变化率为负 且 低值更好
        // 或者
        // 当前值已经为最大值 且 变化率为正 且 高值更好
        // 则不显示箭头
        if (value <= 0 && changeRate < 0 && lowerIsBetter ||
            value >= maxValue && changeRate > 0 && higherIsBetter)
        {
            arrow.rectTransform.DOKill();
            arrow.gameObject.SetActive(false);
            return;
        }

        if (curChangeLavel == lastLevel) return;

        arrow.gameObject.SetActive(true);
        arrow.rectTransform.DOKill();

        arrow.sprite = arrowSprites[Mathf.Abs(curChangeLavel) - 1];
        arrow.transform.localEulerAngles = new Vector3(percentage > 0 ? 0 : 180, 0, 0);

        float duration = .35f;
        if (percentage > 0)
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