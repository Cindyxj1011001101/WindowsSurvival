using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardEvent", menuName = "ScritableObject/CardEvent")]
public class CardEvent:ScriptableObject
{
        public string EventName;
        public string EventDesc;
        public int Time;
        public List<EventTrigger> eventList;
}