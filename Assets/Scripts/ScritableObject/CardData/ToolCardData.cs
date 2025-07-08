
using UnityEngine;
[CreateAssetMenu(fileName = "ToolCardData", menuName = "ScritableObject/ToolCardData")]
public class ToolCardData:CardData
{
    public float Weight;
    public ToolTag tag;
    public int maxEndurance;
}

public enum ToolTag
{
    Cut
}