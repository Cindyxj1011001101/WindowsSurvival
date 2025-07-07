
using UnityEngine;

[CreateAssetMenu(fileName = "MoveEvent", menuName = "ScritableObject/MoveEvent")]
public class MoveEvent:EventTrigger
{
    public PlaceEnum AimPlace;
    
    public override void EventResolve()
    {
        //TODO：移动至另一地区
    }

    public override void Init()
    {
        return;
    }
}