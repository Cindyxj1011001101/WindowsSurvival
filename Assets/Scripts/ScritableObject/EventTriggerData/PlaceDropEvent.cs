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
        int random = Random.Range(1,AllRepeatDrop);
        foreach (var VARIABLE in RepeatDropList)
        {
            if (random<VARIABLE.DropProb)
            {
                //TODO:掉落单张卡牌
                return;
            }
            random-=VARIABLE.DropProb;
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