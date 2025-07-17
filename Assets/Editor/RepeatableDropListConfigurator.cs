using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConfigurablePopulation
{
    public string cardName; // 卡牌
    public int dropNum; // 掉落数量

    public int curSize; // 当前数量
    public int maxSize; // 最大数量

    public int sizeChangePerRound; // 每回合数量变化
    public int sizeChangeOnCaught; // 捕捞后的数量变化
}

[CreateAssetMenu(fileName = "RepeatableDropListConfigurator", menuName = "ScriptableObject/RepeatableDropListConfigurator")]
public class RepeatableDropListConfigurator : ScriptableObject
{
    public int emptyPopulationSizeChangeOnNotCaught;
    public ConfigurablePopulation emptyPopulation = new();
    public List<ConfigurablePopulation> populationList = new();

    public void GenerateRepeatableDropList()
    {
        List<Population> list = new List<Population>();
        foreach (var population in populationList)
        {
            var card = CardFactory.CreateCard(population.cardName);
            list.Add(new Population()
            {
                card = card,
                dropNum = population.dropNum,
                curSize = population.curSize,
                maxSize = population.maxSize,
                sizeChangeOnCaught = population.sizeChangeOnCaught,
                sizeChangePerRound = population.sizeChangePerRound,
            });
        }

        var empty = new Population
        {
            curSize = emptyPopulation.curSize,
            maxSize = emptyPopulation.maxSize,
            sizeChangePerRound = emptyPopulation.sizeChangePerRound,
        };

        RepeatableDropList result = new()
        {
            emptyPopulation = empty,
            emptyPopulationSizeChangeOnNotCaught = emptyPopulationSizeChangeOnNotCaught,
            populationList = list
        };
        JsonManager.SaveData(result, name + "重复掉落列表");
    }
}