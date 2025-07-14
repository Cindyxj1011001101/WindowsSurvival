using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeLibrary", menuName = "ScritableObject/RecipeLibrary")]
public class ScriptableRecipeLibrary : ScriptableObject
{
    public RecipeType craftType;
    public List<ScriptableRecipe> recipes;

    private void OnValidate()
    {
        foreach (var card in recipes)
        {
            card.craftType = craftType;
        }
    }
}