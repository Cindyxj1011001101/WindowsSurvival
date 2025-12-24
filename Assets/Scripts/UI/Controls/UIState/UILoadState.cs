using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 负重状态
/// </summary>
public class UILoadState : UIStateSlider
{
    public Sprite[] levels;

    public override void SetValue(float curValue, float maxValue, bool playAnim)
    {
        UpdateSliderValue(curValue, maxValue, playAnim);
        int level = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].StateLevel;
        icon.sprite = levels[level];

        var color = ColorManager.LoadColors[level];
        if (button != null)
        {
            button.hoveredColor = color;
        }
        button.currentColor = icon.color = stateNameText.color = valueText.color = slider.fillRect.GetComponent<Image>().color = color;
    }

    protected override void DisplayValueText(float curValue, float maxValue)
    {
        valueText.text = $"{curValue:0.0}/{maxValue / 2}";
    }
}