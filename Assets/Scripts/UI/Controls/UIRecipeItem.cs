using UnityEngine;
using UnityEngine.UI;

public class UIRecipeItem : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Image lockImage;
    [SerializeField] Image adequateImage;
    [SerializeField] Color lockedColor;

    public void DisplayRecipe(Sprite icon, bool locked, bool canCraft)
    {
        this.icon.sprite = icon;
        this.icon.color = locked ? lockedColor : Color.white;
        lockImage.gameObject.SetActive(locked);
        adequateImage.gameObject.SetActive(canCraft);
    }
}