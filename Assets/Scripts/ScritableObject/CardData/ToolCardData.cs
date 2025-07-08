
using UnityEngine;
[CreateAssetMenu(fileName = "ToolCardData", menuName = "ScritableObject/ToolCardData")]
public class ToolCardData:CardData
{
    public ToolTag tag;
    public int maxEndurance;
}

public enum ToolTag
{
    Cut
}