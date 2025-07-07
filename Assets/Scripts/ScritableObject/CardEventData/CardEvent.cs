using System.Collections.Generic;
using UnityEngine;

public class CardEvent:ScriptableObject
{
        public string EventName;
        public string EventDesc;
        public int Time;
        public List<EventTrigger> eventList;
}