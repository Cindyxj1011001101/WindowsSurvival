using UnityEngine;

[CreateAssetMenu(fileName = "ValueEvent", menuName = "ScritableObject/ValueEvent")]
public class ValueEvent:EventTrigger
{
        public StateEnum State;
        public int Value;

        public void ChangeValue()
        {
                EventManager.Instance.TriggerEvent<ChangeStateArgs>(EventType.ChangeState, new ChangeStateArgs()
                {
                        state = State,
                        value = Value
                });
        }
}