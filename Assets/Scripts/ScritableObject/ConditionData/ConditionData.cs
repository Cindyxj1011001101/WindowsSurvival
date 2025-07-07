using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConditionData", menuName = "ScritableObject/ConditionData")]
public class ConditionData:ScriptableObject
{
    //条件之间是和的关系
    public List<CardData> CardDataList;
    public List<CardType> CardTypeList;
    public List<ToolTag> ToolTagList;
    public List<CardTag> CardTagList;
}