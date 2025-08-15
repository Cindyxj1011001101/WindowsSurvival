using UnityEngine;
using UnityEngine.UI;

public class UIRecipeMaterial : MonoBehaviour
{
    public Image icon;
    public Text requiredNumText;
    public HoverableButton button;
    public HoverTipController tipController;

    public void DisplayMaterial(Sprite icon, int requiredNum, int currentNum)
    {
        this.icon.sprite = icon;
        requiredNumText.text = $"{currentNum}/{requiredNum}";
        button.currentColor = this.icon.color = requiredNumText.color = currentNum < requiredNum ? ColorManager.LightGrey : ColorManager.White;
    }
}