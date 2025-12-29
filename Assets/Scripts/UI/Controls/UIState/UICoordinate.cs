using DG.Tweening;
using UnityEngine.UI;

public class UICoordinate : UIStateSlider
{
    public Slider playerCoordSlider;

    protected override void OnDisable()
    {
        base.OnDisable();
        if (playerCoordSlider != null) playerCoordSlider.DOKill();
    }

    public override void SetValue(float curValue, float maxValue, bool playAnim)
    {
        UpdateSliderValue(curValue,  maxValue, playAnim);
        // 显示玩家位置
        var playerPos = Player.Instance.Coordinate.Position / maxValue;
        if (playAnim)
        {
            playerCoordSlider.DOKill();
            playerCoordSlider.DOValue(playerPos, valueTransition);
        }
        else
        {
            playerCoordSlider.value = playerPos;
        }
    }

    protected override void DisplayValueText(float curValue, float maxValue)
    {
        valueText.text = $"{curValue:0.0}";
    }
}