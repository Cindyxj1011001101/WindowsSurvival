using UnityEngine;

[CreateAssetMenu(fileName = "PlaceData", menuName = "ScritableObject/PlaceData")]
public class PlaceData : ScriptableObject
{
    public string placeName;
    public string placeDesc;
    public PlaceEnum placeType;
}