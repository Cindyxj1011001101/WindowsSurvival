using UnityEngine;
using UnityEngine.UI;

public class UIRecipeMaterial : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Image isEnough;
    [SerializeField] private Text amount;
    [SerializeField] private Color notEnoughColor;
    public void DisplayMaterial(Sprite icon, int requiredAmount, int currentAmount)
    {
        this.icon.sprite = icon;
        amount.text = $"{currentAmount} / {requiredAmount}";
        if (currentAmount < requiredAmount)
        {
            isEnough.gameObject.SetActive(true);
            amount.color = notEnoughColor;
        }
        else
        {
            isEnough.gameObject.SetActive(false);
            amount.color = Color.white;
        }
    }
}