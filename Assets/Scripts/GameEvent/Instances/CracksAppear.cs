using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 出现裂缝
/// </summary>
public class CracksAppear : GameEvent
{
    public override string GetDetails()
    {
        return "麦麦听到了一声极其尖锐、高亢的撕裂声，紧接着是沉闷的爆裂声。这个声音非常熟悉，似乎是哪里又出现裂缝了。";
    }

    public override bool CanTriggerThisEvent()
    {
        // 条件是当前地点在飞船内或者飞船外壳
        return GameManager.Instance.CurEnvironmentBag.PlaceData.isInSpacecraft ||
            GameManager.Instance.CurEnvironmentBag.PlaceData.placeType == PlaceEnum.SpaceshipOuterHull;
    }

    public override void OnTrigger()
    {
        // 随机一个飞船内地点
        var envs = GameManager.Instance.EnvironmentBags.Values.Where(e => e.PlaceData.isInSpacecraft).ToArray();
        var targetEnv = envs[Random.Range(0, envs.Length)];

        // 随机裂缝个数
        var crackCount = Random.Range(1, 4); // 随机1~3个裂缝

        // 加入到目标地点
        var cards = new List<Card>();
        for (int i = 0; i < crackCount; i++)
        {
            cards.Add(CardFactory.CreateCard("渗水裂缝"));
        }
        GameManager.Instance.AddCardsToTargetEnv(cards, targetEnv);

        Debug.Log($"在{targetEnv.PlaceName}生成了{crackCount}个渗水裂缝");
    }
}
