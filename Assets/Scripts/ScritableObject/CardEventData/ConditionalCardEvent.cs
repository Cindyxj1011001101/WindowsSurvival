using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConditionCardEvent", menuName = "ScritableObject/ConditionCardEvent")]
public class ConditionalCardEvent:CardEvent
{
        public List<ConditionData> ConditionCardList;//list之间是或的关系
}