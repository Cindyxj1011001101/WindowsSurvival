using System.Collections.Generic;
using UnityEngine;

public enum TechType
{
    Construction,
    Food,
    Oxygen,
    Tool,
    Resource,
    Combat,
}

public enum TechLevl
{
    Junior, // 初级
    Intermediate, // 中级
    Advanced // 高级
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
    public TechLevl techLevel;

    private void OnValidate()
    {
        techName = name;
    }
}