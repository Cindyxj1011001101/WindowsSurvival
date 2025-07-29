using System.Collections.Generic;
using UnityEngine;

public enum TechType
{
    Food,
    Oxygen,
    Equipment,
    Construction,
    Resource,
}

[CreateAssetMenu(fileName = "Technology", menuName = "ScriptableObject/Technology")]
public class ScriptableTechnologyNode : ScriptableObject
{
    public string techName; // 科技的名称
    [TextArea]
    public string techDescription; // 科技的详细描述
    public List<ScriptableRecipe> recipes; // 解锁的配方
    public List<ScriptableTechnologyNode> prerequisites; // 前置科技条件
    public int cost; // 需要消耗的科技点
    public TechType techType;

    private void OnValidate()
    {
        techName = name;
    }
}