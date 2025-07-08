using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlaceDropEvent", menuName = "ScritableObject/PlaceDropEvent")]
public class PlaceDropEvent:EventTrigger
{
    public PlaceEnum PlaceManager;
    public List<Drop> DefaultList;
    public List<Drop> OnceDropList;
    public List<Drop> RepeatDropList;
    public int AllRepeatDrop;
    //重复掉落逻辑
    public override void EventResolve()
    {
        //TODO:处理没有可重复掉落卡牌的情况
        int random = Random.Range(1,AllRepeatDrop);
        foreach (var drop in RepeatDropList)
        {
            if (random<drop.DropProb)
            {
                EventManager.Instance.TriggerEvent(EventType.AddDropCard, drop);
                return;
            }
            random-=drop.DropProb;
        }
    }

    public override void Init()
    {
        AllRepeatDrop = 0;
        foreach (var VARIABLE in RepeatDropList)
        {
            AllRepeatDrop+=VARIABLE.DropProb;
        }
        return;
    }
}