using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class ConfigurableDrop
{
    public string cardName;
    public int dropNum;
    public int dropProb;
}


[CreateAssetMenu(fileName = "DisposableDropListConfigurator", menuName = "ScriptableObject/DisposableDropListConfigurator")]
public class DisposableDropListConfigurator : ScriptableObject
{
    public List<ConfigurableDrop> dropList;

    public void GenerateDisposableDropList()
    {
        List<Drop> list = new List<Drop>();
        foreach (var drop in dropList)
        {
            var card = CardFactory.CreateCard(drop.cardName);
            list.Add(new Drop()
            {
                card = card,
                dropNum = drop.dropNum,
                dropProb = drop.dropProb
            });
        }

        DisposableDropList result = new() { maxCount = list.Count, dropList = list };
        JsonManager.SaveData(result, name + "一次性掉落列表");
    }
}