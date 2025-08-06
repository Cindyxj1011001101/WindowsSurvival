using UnityEngine;
using UnityEngine.UI;

public class UIRecipeItem : MonoBehaviour
{
    public Image icon;
    public Image lockImage;
    public Image adequateImage;

    public Sprite normalLockSprite;
    public Sprite hoveredLockSprite;
    public Sprite normalAdequateSprite;
    public Sprite hoveredAdequateSprite;

    public HoverableButton button;

    private void Awake()
    {
        button.onPointerEnter.AddListener(() =>
        {
            lockImage.sprite = hoveredLockSprite;
            adequateImage.sprite = normalAdequateSprite;
        });
        button.onPointerExit.AddListener(() =>
        {
            lockImage.sprite = normalLockSprite;
            adequateImage.sprite = hoveredAdequateSprite;
        });
    }

    public void DisplayRecipe(Sprite icon, bool locked, bool canCraft)
    {
        this.icon.sprite = icon;

        if (locked)
            button.currentColor = this.icon.color = ColorManager.DarkGrey;
        else if (canCraft)
            button.currentColor = this.icon.color = ColorManager.White;
        else
            button.currentColor = this.icon.color = ColorManager.LightGrey;

        lockImage.gameObject.SetActive(locked);
        adequateImage.gameObject.SetActive(canCraft);
    }
}