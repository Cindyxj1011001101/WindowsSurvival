using UnityEngine;
public abstract class EventTrigger:ScriptableObject
{
    public abstract void Invoke();
    public abstract void Init();
}