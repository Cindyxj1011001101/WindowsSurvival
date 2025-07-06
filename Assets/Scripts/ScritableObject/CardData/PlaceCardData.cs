
using UnityEngine;
[CreateAssetMenu(fileName = "PlaceCardData", menuName = "ScritableObject/PlaceCardData")]
public class PlaceCardData:CardData
{
    public override void Init()
    { 
        cardType=CardType.Place;
    }
}
