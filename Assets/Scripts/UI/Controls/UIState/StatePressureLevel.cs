using UnityEngine;
using UnityEngine.UI;

public class StatePressureLevel : MonoBehaviour
{
    public Text pressureLevelText;

    public Image[] levels;

    public Color[] colors;

    public void SetValue(PressureLevel level)
    {
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < (int)level; i++)
        {
            levels[i].gameObject.SetActive(true);
            levels[i].color = colors[(int)level];
        }

        pressureLevelText.text = ParsePressureLevel(level);

        pressureLevelText.color = colors[(int)level];
    }

    private string ParsePressureLevel(PressureLevel level)
    {
        return level switch
        {
            PressureLevel.VeryLow => "极低压强",
            PressureLevel.Low => "低压强",
            PressureLevel.Standard => "标准压强",
            PressureLevel.High => "高压强",
            PressureLevel.VeryHigh => "极高压强",
            _ => "未知",
        };
    }
}