
using UnityEngine;
[CreateAssetMenu(fileName = "ResourceCardData", menuName = "ScritableObject/ResourceCardData")]
public class ResourceCardData:CardData
{
    public float Weight;
    public override void Init()
    { 
        cardType=CardType.Resource;
    }
}