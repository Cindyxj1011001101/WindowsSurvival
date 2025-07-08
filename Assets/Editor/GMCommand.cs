using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GMCommand
{
    [MenuItem("Command/添加一块压缩饼干")]
    public static void A()
    {
        var card = CardFactory.CreateCardInstance<FoodCardInstance>("压缩饼干");
        Debug.Log(card);
        PlayerBag playerBag = Object.FindObjectOfType<PlayerBag>();
        playerBag.AddCard(card);
    }

    [MenuItem("Command/添加一块废金属")]
    public static void B()
    {
        var card = CardFactory.CreateCardInstance<ResourceCardInstance>("废金属");
        Debug.Log(card);
        PlayerBag playerBag = Object.FindObjectOfType<PlayerBag>();
        playerBag.AddCard(card);
    }
}
