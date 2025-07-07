
using UnityEngine;
[CreateAssetMenu(fileName = "ToolCardData", menuName = "ScritableObject/ToolCardData")]
public class ToolCardData:CardData
{
    public float Weight;
    public ToolTag tag;
    public int maxEndurance;
    public override void Init()
    { 
        cardType=CardType.Tool;
    }
}

public enum ToolTag
{
    Cut
}