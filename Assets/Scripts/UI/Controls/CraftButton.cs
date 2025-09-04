using UnityEngine;
using UnityEngine.UI;

public  class CraftButton : HoverableButton
{
    public Text text;
    public GameObject iconObject;

    //private HoverTipController hoverTipController;

    protected override void Awake()
    {
        base.Awake();
        //hoverTipController = gameObject.AddComponent<HoverTipController>();
    }

    public void DisplayButton(bool isLocked, bool canCraft, string hint)
    {
        if (isLocked)
        {
            Interactable = false;
            iconObject.SetActive(false);
            text.text = "未解锁";
            text.color = ColorManager.DarkGrey;
            //hoverTipController.enabled = false;
        }
        else if (canCraft)
        {
            Interactable = true;
            iconObject.SetActive(true);
            text.text = "开始制作";
            text.color = ColorManager.White;
            //hoverTipController.enabled = false;
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
            //hoverTipController.enabled = true;
        }

        //hoverTipController.SetTip(hint);
    }
}