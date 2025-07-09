using System;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class GMCommand
{
    private static void AddCard(string cardName)
    {
        string dataPath = "ScriptableObject/Card/" + cardName;
        CardData defaultData = Resources.Load<CardData>(dataPath);
        var card = CardFactory.CreateCardIntance(defaultData);
        var window = WindowsManager.Instance.GetCurrentFocusedWindow();
        if (window == null || window is not BagWindow) return;
        (window as BagWindow).AddCard(card);
        Debug.Log(card);
    }

    [MenuItem("Command/添加压缩饼干")]
    public static void A()
    {
        AddCard("压缩饼干");
    }

    [MenuItem("Command/添加废金属")]
    public static void B()
    {
        AddCard("废金属");
    }

    [MenuItem("Command/添加瓶装水")]
    public static void C()
    {
        AddCard("瓶装水");
    }

    [MenuItem("Command/添加通往驾驶室的门")]
    public static void D()
    {
        AddCard("通往驾驶室的门");
    }

    [MenuItem("Command/添加通往动力舱的门")]
    public static void E()
    {
        AddCard("通往动力舱的门");
    }

    [MenuItem("Command/添加被安全泡沫覆盖的废料堆")]
    public static void F()
    {
        AddCard("被安全泡沫覆盖的废料堆");
    }

    [MenuItem("Command/添加硬质纤维")]
    public static void G()
    {
        AddCard("硬质纤维");
    }

    [MenuItem("Command/添加老鼠尸体")]
    public static void H()
    {
        AddCard("老鼠尸体");
    }

    [MenuItem("Command/添加小块肉")]
    public static void I()
    {
        AddCard("小块肉");
    }

    [MenuItem("Command/添加废铁刀")]
    public static void J()
    {
        AddCard("废铁刀");
    }

    [MenuItem("Command/添加腐烂物")]
    public static void K()
    {
        AddCard("腐烂物");
    }

    [MenuItem("Command/保存玩家背包")]
    public static void SavePlayerBag()
    {
        string dataPath = "ScriptableObject/Card/腐烂物";
        CardData defaultData = Resources.Load<CardData>(dataPath);
        var card = CardFactory.CreateCardIntance(defaultData);
        string json = JsonConvert.SerializeObject(card);
        Debug.Log(json);
    }
}
