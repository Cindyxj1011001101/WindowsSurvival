using System.Collections.Generic;
using UnityEngine;

public class LoveBead : Card
{
    public int maxProductProcess;//最大生产进度
    public int curProductProcess;//当前生产进度
    public int maxProductNum;//最大生产数量
    public int curProductNum;//当前生产数量
    public int maxFresh;//最大新鲜度
    public int curFresh;//当前新鲜度

    public LoveBead()
    {
        cardName = "爱情贝";
        cardDesc = "形似扇贝的古怪异星生物。其强大的肌肉使其能像鱼一样扇动贝壳游动。繁殖期的雄贝会搜集各类珍宝藏在贝壳中以吸引雌贝。";
        cardType = CardType.Creature;
        maxStackNum = 5;
        moveable = true;
        maxProductProcess = 900;
        curProductProcess = 0;
        maxProductNum = 1;
        curProductNum = 0;
        maxFresh = 2160;
        curFresh = 2160;
        weight = 1.5f;
        events = new List<Event>()
        {
            new Event("取贝肉", "取贝肉", Event_GetMeat, null)
        };
        tags = new List<CardTag>();
        components = new()
        {
            { typeof(FreshnessComponent), new FreshnessComponent(2160, OnFreshnessChanged) },
            { typeof(ProgressComponent), new ProgressComponent(3600, OnProgressFull) },
        };
    }
    #region 事件
    public void Event_GetMeat()
    {
        Use();
        TimeManager.Instance.AddTime(30);
        GameManager.Instance.AddCard(new RawOysterMeat(), true);
        GameManager.Instance.AddCard(new RawOysterMeat(), true);
    }

    #endregion

    #region 组件
    private void OnFreshnessChanged(int freshness)
    {
        Debug.Log("当前新鲜度：" + freshness);
    }

    private void OnProgressFull()
    {
        Use();
        TryGetComponent<ProgressComponent>(out var component);
        GameManager.Instance.AddCard(new LoveBeadWithProduct(component.progress), true);
    }
    #endregion
}