
using UnityEngine;

[CreateAssetMenu(fileName = "MoveEvent", menuName = "ScritableObject/MoveEvent")]
public class MoveEvent:EventTrigger
{
    public PlaceEnum AimPlace;
    
    public override void Invoke()
    {
        
        GameManager.Instance.Move(AimPlace);
    }

    public override void Init()
    {
        return;
    }
}