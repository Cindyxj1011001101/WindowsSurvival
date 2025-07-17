using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Technology", menuName = "ScriptableObject/Technology")]
public class ScriptableTechnologyNode : ScriptableObject
{
    public string techName; // 科技的名称
    public string techDescription; // 科技的详细描述
    public List<ScriptableRecipe> recipes; // 解锁的配方
    public List<ScriptableTechnologyNode> prerequisites; // 前置科技条件
    //public float progress; // 当前进度
    public int cost; // 需要消耗的科技点

    private void OnValidate()
    {
        techName = name;
    }
}