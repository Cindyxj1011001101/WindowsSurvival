using UnityEngine;
public abstract class EventTrigger:ScriptableObject
{
    public abstract void EventResolve();
    public abstract void Init();
}