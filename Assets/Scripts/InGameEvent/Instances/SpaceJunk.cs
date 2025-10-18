using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 太空垃圾
/// </summary>
public class SpaceJunk: InGameEvent
{
    private List<PlaceEnum> candidatePlaces = new()
    {
        PlaceEnum.CoralCoast,
        PlaceEnum.PhosphorTomb,
    };

    private DropList dropList = new(
        new Drop(35, "废金属", 5, 10),
        new Drop(35, "韧性胶管", 3, 8),
        new Drop(4, "氧烛", 1, 3),
        new Drop(20, "燃素", 2, 6),
        new Drop(5, "小块生肉", 2, 5),
        new Drop(4, "电池", 1, 3),
        new Drop(1, "钢锤", 1),
        new Drop(3, "废铁刀", 1),
        new Drop(1, "铁齿铜牙餐", 1),
        new Drop(1, "炸虫串", 1)
        ) { disposable = true };

    public override void TriggerThisEvent()
    {
        // 随机一个地点
        var placeType = candidatePlaces[Random.Range(0, candidatePlaces.Count)];
        var env = GameManager.Instance.EnvironmentBags[placeType];

        var junkPackage = CardFactory.CreateCard("垃圾包裹");
        junkPackage.TryGetComponent<InnerContentsComponent>(out var innerContents);

        // 拷贝掉落列表
        var actualDropList = JsonManager.DeepCopy(dropList);

        // 随机抽取2~4种物资
        var count = Random.Range(2, 5);
        for (int i = 0; i < count; i++)
        {
            foreach (var card in actualDropList.RandomDrop(out _))
            {
                innerContents.AddCard(card);
            }
        }

        // 掉落到地点
        GameManager.Instance.AddCardsToTargetEnv(env, junkPackage);
    }
}
