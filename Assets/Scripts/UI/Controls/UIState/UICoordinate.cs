using UnityEngine.UI;

public class UICoordinate : UIStateSlider
{
    public Slider playerCoordSlider;

    public override void SetValue(float value, float maxValue)
    {
        slider.value = value / maxValue;
        playerCoordSlider.value = GameManager.Instance.Player.Coordinate.Position / maxValue;

        valueText.text = $"{value:0.0}";
    }
}