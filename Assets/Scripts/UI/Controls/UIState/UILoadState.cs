using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 负重状态
/// </summary>
public class UILoadState : UIStateSlider
{
    public Sprite[] levels;
    public Image icon;
    public Color[] colors;

    public HoverableButton button;

    public override void SetValue(float value, float maxValue)
    {
        slider.value = value / maxValue;
        valueText.text = $"{value:0.0}/{maxValue / 2}";
        int level = StateManager.Instance.GetLoadLevel();
        icon.sprite = levels[level];

        var color = colors[level];
        if (button != null)
        {
            button.hoveredColor = color;
        }
        stateNameText.color = valueText.color = slider.fillRect.GetComponent<Image>().color = color;
    }
}