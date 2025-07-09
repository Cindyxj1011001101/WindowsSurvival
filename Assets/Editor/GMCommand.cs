using UnityEditor;
using UnityEngine;

public class GMCommand
{
    [MenuItem("Command/添加压缩饼干")]
    public static void A()
    {
        var card = CardFactory.CreateCardInstance<FoodCardInstance>("压缩饼干");
        Debug.Log(card);
        PlayerBag playerBag = Object.FindObjectOfType<PlayerBag>();
        playerBag.AddCard(card);
    }

    [MenuItem("Command/添加废金属")]
    public static void B()
    {
        var card = CardFactory.CreateCardInstance<ResourceCardInstance>("废金属");
        Debug.Log(card);
        PlayerBag playerBag = Object.FindObjectOfType<PlayerBag>();
        playerBag.AddCard(card);
    }

    [MenuItem("Command/添加瓶装水")]
    public static void C()
    {
        var card = CardFactory.CreateCardInstance<FoodCardInstance>("瓶装水");
        Debug.Log(card);
        PlayerBag playerBag = Object.FindObjectOfType<PlayerBag>();
        playerBag.AddCard(card);
    }

    [MenuItem("Command/添加通往驾驶室的门")]
    public static void D()
    {
        var card = CardFactory.CreateCardInstance<PlaceCardInstance>("通往驾驶室的门");
        Debug.Log(card);
        PlayerBag playerBag = Object.FindObjectOfType<PlayerBag>();
        playerBag.AddCard(card);
    }

    [MenuItem("Command/添加通往动力舱的门")]
    public static void E()
    {
        var card = CardFactory.CreateCardInstance<PlaceCardInstance>("通往动力舱的门");
        Debug.Log(card);
        PlayerBag playerBag = Object.FindObjectOfType<PlayerBag>();
        playerBag.AddCard(card);
    }

    [MenuItem("Command/添加被安全泡沫覆盖的废料堆")]
    public static void F()
    {
        var card = CardFactory.CreateCardInstance<ResourcePointCardInstance>("被安全泡沫覆盖的废料堆");
        Debug.Log(card);
        PlayerBag playerBag = Object.FindObjectOfType<PlayerBag>();
        playerBag.AddCard(card);
    }

    [MenuItem("Command/添加硬质纤维")]
    public static void G()
    {
        var card = CardFactory.CreateCardInstance<ResourceCardInstance>("硬质纤维");
        Debug.Log(card);
        PlayerBag playerBag = Object.FindObjectOfType<PlayerBag>();
        playerBag.AddCard(card);
    }

    [MenuItem("Command/添加老鼠尸体")]
    public static void H()
    {
        var card = CardFactory.CreateCardInstance<FoodCardInstance>("老鼠尸体");
        Debug.Log(card);
        PlayerBag playerBag = Object.FindObjectOfType<PlayerBag>();
        playerBag.AddCard(card);
    }

    [MenuItem("Command/添加小块肉")]
    public static void I()
    {
        var card = CardFactory.CreateCardInstance<FoodCardInstance>("小块肉");
        Debug.Log(card);
        PlayerBag playerBag = Object.FindObjectOfType<PlayerBag>();
        playerBag.AddCard(card);
    }

    [MenuItem("Command/添加废铁刀")]
    public static void J()
    {
        var card = CardFactory.CreateCardInstance<ToolCardInstance>("废铁刀");
        Debug.Log(card);
        PlayerBag playerBag = Object.FindObjectOfType<PlayerBag>();
        playerBag.AddCard(card);
    }

    [MenuItem("Command/添加腐烂物")]
    public static void K()
    {
        var card = CardFactory.CreateCardInstance<FoodCardInstance>("腐烂物");
        Debug.Log(card);
        PlayerBag playerBag = Object.FindObjectOfType<PlayerBag>();
        playerBag.AddCard(card);
    }
    [MenuItem("Command/健康+10")]
    public static void L()
    {
       TimeManager.Instance.AddTime(50);
    }
}
