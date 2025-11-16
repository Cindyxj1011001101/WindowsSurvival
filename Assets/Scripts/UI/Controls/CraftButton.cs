using UnityEngine;

public  class CraftButton : HoverableButton
{
    public GameObject iconObject;

    public void DisplayButton(bool isLocked, bool canCraft, string hint)
    {
        if (isLocked)
        {
            Interactable = false;
            iconObject.SetActive(false);
            text.text = "未解锁";
            text.color = ColorManager.DarkGrey;
        }
        else if (canCraft)
        {
            Interactable = true;
            iconObject.SetActive(true);
            text.text = "开始制作";
            text.color = ColorManager.White;
        }
        else
        {
            Interactable = false;
            iconObject.SetActive(false);
            if (string.IsNullOrEmpty(hint))
                text.text = "不可制作";
            else
                text.text = hint;
            text.color = ColorManager.LightGrey;
        }
    }
}