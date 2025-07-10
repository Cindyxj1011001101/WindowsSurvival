using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlaceDropEvent", menuName = "ScritableObject/PlaceDropEvent")]
public class PlaceDropEvent:EventTrigger
{
    public PlaceEnum PlaceManager;
    public List<Drop> DefaultList;
    public List<Drop> OnceDropList;
    public List<Drop> RepeatDropList;
    public List<Drop> curOnceDropList;
    public int AllRepeatDrop;
    //掉落-场景掉落（掉一次后不掉/重复掉）
    public override void EventResolve()
    {
        if (curOnceDropList.Count != 0)
        {
            int sumProb=0;
            foreach (var drop in curOnceDropList)
            {
                sumProb += drop.DropProb;
            }
            int rand = Random.Range(0, sumProb);
            foreach (var drop in curOnceDropList)
            {
                if (rand < drop.DropProb)
                {
                    EffectResolve.Instance.AddDropCard(drop,false);
                    curOnceDropList.Remove(drop);
                    return;
                }
                rand -= drop.DropProb;
            }
        }
        else
        {
            if (RepeatDropList != null)
            {
                int random = Random.Range(1,AllRepeatDrop);
                foreach (var drop in RepeatDropList)
                {
                    if (random<drop.DropProb)
                    {
                        EventManager.Instance.TriggerEvent<AddDropCardArgs>(EventType.AddDropCard,new AddDropCardArgs(drop,false));
                        return;
                    }
                    random-=drop.DropProb;
                }
            }
        }
        
    }

    public override void Init()
    {
        AllRepeatDrop = 0;
        curOnceDropList = new List<Drop>(OnceDropList);
        foreach (var VARIABLE in RepeatDropList)
        {
            AllRepeatDrop+=VARIABLE.DropProb;
        }
        return;
    }
}