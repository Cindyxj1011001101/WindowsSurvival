
using UnityEngine;
[CreateAssetMenu(fileName = "ToolCardData", menuName = "ScritableObject/ToolCardData")]
public class ToolCardData:CardData
{
    public ToolTag tag;
}

public enum ToolTag
{
    Cut
}