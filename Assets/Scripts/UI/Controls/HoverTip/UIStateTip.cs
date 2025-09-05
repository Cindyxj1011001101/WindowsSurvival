using UnityEngine;
using UnityEngine.UI;

public class UIStateTip : MonoBehaviour
{
    public Text valueText;
    public Slider front;
    public Slider behind;

    public virtual void SetValue(float value, float maxValue, float delta)
    {
        if (delta > 0)
        {
            front.value = value / maxValue;
            behind.value = (value + delta) / maxValue;
            valueText.text = $"+{delta}";
            valueText.color = behind.fillRect.GetComponent<Image>().color = ColorManager.Green;
        }
        else
        {
            front.value = (value + delta) / maxValue;
            behind.value = value / maxValue;
            valueText.text = $"-{-delta}";
            valueText.color = behind.fillRect.GetComponent<Image>().color = ColorManager.Red;
        }
    }

    public void SetValue(State state, float delta)
    {
        SetValue(state.CurValue, state.MaxValue, delta);
    }
}