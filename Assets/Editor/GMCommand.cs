using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GMCommand
{
    [MenuItem("Command/添加一个压缩饼干到背包")]
    public static void 添加一个压缩饼干到背包()
    {
        var card = CardFactory.CreateCardInstance<FoodCardInstance>("压缩饼干");
        Debug.Log(card);
        PlayerBag playerBag = Object.FindObjectOfType<PlayerBag>();
        playerBag.AddCard(card);
    }

    [MenuItem("Command/添加一块废金属到背包")]
    public static void 添加一块废金属到背包()
    {
        var card = CardFactory.CreateCardInstance<ResourceCardInstance>("废金属");
        Debug.Log(card);
        PlayerBag playerBag = Object.FindObjectOfType<PlayerBag>();
        playerBag.AddCard(card);
    }

    [MenuItem("Command/添加一个硬质纤维到背包")]
    public static void 添加一个硬质纤维到背包()
    {

    }

    [MenuItem("Command/添加一个老鼠尸体到背包")]
    public static void 添加一个老鼠尸体到背包()
    {

    }
}
