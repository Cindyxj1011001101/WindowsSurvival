using UnityEngine;

[CreateAssetMenu(fileName = "ValueEvent", menuName = "ScritableObject/ValueEvent")]
public class ValueEvent:EventTrigger
{
        public StateEnum State;
        public int Value;

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