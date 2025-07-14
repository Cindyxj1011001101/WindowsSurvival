using UnityEngine;
using UnityEngine.UI;

public class UIRecipeButton : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Image canCraft;
    [SerializeField] Image isLocked;

    public void DisplayRecipe(Sprite icon, bool locked, bool canCraft)
    {
        this.icon.sprite = icon;
        this.isLocked.gameObject.SetActive(locked);
        if (locked)
            this.canCraft.gameObject.SetActive(false);
        else
            this.canCraft.gameObject.SetActive(!canCraft);
    }
}