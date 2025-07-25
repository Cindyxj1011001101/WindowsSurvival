using UnityEngine;
using UnityEngine.UI;

public  class CraftButton : HoverableButton
{
    public Text text;
    public GameObject iconObject;
    public Color normalColor;
    public Color cannotCraftColor;
    public Color lockedColor;

    public void DisplayButton(bool isLocked, bool canCraft)
    {
        if (isLocked)
        {
            Interactable = false;
            iconObject.SetActive(false);
            text.text = "未解锁";
            text.color = lockedColor;
        }
        else if (canCraft)
        {
            Interactable = true;
            iconObject.SetActive(true);
            text.text = "开始制作";
            text.color = normalColor;
        }
        else
        {
            Interactable = false;
            iconObject.SetActive(false);
            text.text = "缺少材料";
            text.color = cannotCraftColor;
        }
    }
}