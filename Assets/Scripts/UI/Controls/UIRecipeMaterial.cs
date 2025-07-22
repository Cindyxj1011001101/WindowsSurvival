using UnityEngine;
using UnityEngine.UI;

public class UIRecipeMaterial : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text requiredNumText;
    [SerializeField] private Color notAdequateColor;

    public void DisplayMaterial(Sprite icon, int requiredNum, int currentNum)
    {
        this.icon.sprite = icon;
        requiredNumText.text = $"{currentNum}/{requiredNum}";
        this.icon.color = requiredNumText.color = currentNum < requiredNum ? notAdequateColor : Color.white;
    }
}