using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 负重状态
/// </summary>
public class UILoadState : UIStateSlider
{
    public Sprite[] levels;
    public Color[] colors;

    public override void SetValue(float value, float maxValue)
    {
        slider.value = value / maxValue;
        valueText.text = $"{value:0.0}/{maxValue / 2}";
        int level = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].StateLevel;
        icon.sprite = levels[level];

        var color = colors[level];
        if (button != null)
        {
            button.hoveredColor = color;
        }
        stateNameText.color = valueText.color = slider.fillRect.GetComponent<Image>().color = color;
    }
}