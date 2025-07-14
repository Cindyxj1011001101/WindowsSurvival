using UnityEngine;

[CreateAssetMenu(fileName = "ValueEvent", menuName = "ScritableObject/ValueEvent")]
public class ValueEvent:EventTrigger
{
        public PlayerStateEnum State;
        public float Value;

        public override void Invoke()
        {
                EventManager.Instance.TriggerEvent<ChangeStateArgs>(EventType.ChangeState,
                        new ChangeStateArgs(State, Value));
        }

        public override void Init()
        {
                return;
        }
}