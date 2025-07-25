using UnityEngine;
using UnityEngine.UI;

public class UIRecipeMaterial : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text requiredNumText;
    [SerializeField] private HoverableButton button;

    public void DisplayMaterial(Sprite icon, int requiredNum, int currentNum)
    {
        this.icon.sprite = icon;
        requiredNumText.text = $"{currentNum}/{requiredNum}";
        button.currentColor = this.icon.color = requiredNumText.color = currentNum < requiredNum ? ColorManager.Instance.lightGrey : ColorManager.Instance.white;
    }
}