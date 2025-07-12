using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlaceDropEvent", menuName = "ScritableObject/PlaceDropEvent")]
public class PlaceDropEvent:EventTrigger
{
    public PlaceEnum Place;
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
                    GameManager.Instance.AddDropCard(drop,false);
                    curOnceDropList.Remove(drop);
                    //处理探索度变化
                    float explore =(1- (float)curOnceDropList.Count / OnceDropList.Count)*100;
                    EventManager.Instance.TriggerEvent<ChangeDiscoveryDegreeArgs>(EventType.ChangeDiscoveryDegree,new ChangeDiscoveryDegreeArgs(Place,explore));
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

    public void DropCertainPlaceCard(PlaceEnum place)
    {
        foreach (var drop in curOnceDropList)
        {
            if (drop.GetCardData() is PlaceCardData placeCardData && placeCardData.place == place)
            {
                GameManager.Instance.AddDropCard(drop,false);
                curOnceDropList.Remove(drop);
                //处理探索度变化
                float explore =(1- (float)curOnceDropList.Count / OnceDropList.Count)*100;
                EventManager.Instance.TriggerEvent<ChangeDiscoveryDegreeArgs>(EventType.ChangeDiscoveryDegree,new ChangeDiscoveryDegreeArgs(Place,explore));
                return;
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