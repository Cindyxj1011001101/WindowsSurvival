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
        if (!canCraft)
        {
            this.canCraft.gameObject.SetActive(false);
            this.isLocked.gameObject.SetActive(false);
        }
        else
        {
            isLocked.gameObject.SetActive(locked);
        }
    }
}