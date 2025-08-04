using UnityEngine;
using UnityEngine.UI;

public class DialogueOption : HoverableButton
{
    public Text optionText;

    public RectTransform rectTransform;

    public void SetText(string text)
    {
        optionText.text = text;

        var textRectTransform = optionText.transform as RectTransform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(textRectTransform);
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, textRectTransform.sizeDelta.y + 4);
    }
}