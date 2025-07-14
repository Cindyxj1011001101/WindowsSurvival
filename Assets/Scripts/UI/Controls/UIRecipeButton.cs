using UnityEngine;
using UnityEngine.UI;

public class UIRecipeButton : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Image canCraftImage;
    [SerializeField] Image lockImage;

    public void DisplayRecipe(Sprite icon, bool locked, bool canCraft)
    {
        this.icon.sprite = icon;
        lockImage.gameObject.SetActive(locked);
        if (locked)
            canCraftImage.gameObject.SetActive(false);
        else
            canCraftImage.gameObject.SetActive(!canCraft);
    }
}