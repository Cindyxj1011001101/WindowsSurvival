using UnityEngine;
using UnityEngine.UI;

public class StatePressureLevel : MonoBehaviour
{
    public Text pressureLevelText;

    public Image[] levels;

    public Color[] colors;

    public void SetValue(PressureLevel level)
    {
        // 压强对应的颜色
        var color = colors[(int)level - 1];

        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].gameObject.SetActive(false);
        }

        // 显示压强等级，一个level物体代表一个等级
        for (int i = 0; i < (int)level; i++)
        {
            levels[i].gameObject.SetActive(true);
            levels[i].color = color;
        }

        // 显示压强数值
        pressureLevelText.text = ParsePressureLevel(level);
        pressureLevelText.color = color;
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