using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeLibrary", menuName = "ScriptableObject/RecipeLibrary")]
public class ScriptableRecipeLibrary : ScriptableObject
{
    public string libraryName;
    public RecipeType craftType;
    public List<ScriptableRecipe> recipes;

    private void OnValidate()
    {
        libraryName = name;
        foreach (var card in recipes)
        {
            card.craftType = craftType;
        }
    }
}