using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public  class CraftButton : HoverableButton
{
    [SerializeField] GameObject craftObject;
    [SerializeField] private Text craftText;
    [SerializeField] private Text lockText;

    public void DisplayButton(bool isLocked, bool canCraft)
    {
        craftText.color = Color.white;
        if (isLocked)
        {
            enabled = false;
            craftObject.SetActive(false);
            lockText.gameObject.SetActive(true);
        }
        else if (canCraft)
        {
            enabled = true;
            craftObject.SetActive(true);
            lockText.gameObject.SetActive(false);
        }
        else
        {
            enabled = false;
            craftObject.SetActive(true);
            lockText.gameObject.SetActive(false);
        }
    }
}