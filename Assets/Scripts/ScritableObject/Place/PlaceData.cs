using UnityEngine;

[CreateAssetMenu(fileName = "PlaceData", menuName = "ScritableObject/PlaceData")]
public class PlaceData : ScriptableObject
{
    public string placeName;
    public string placeDesc;
    public PlaceEnum placeType;
    public bool isIndoor;
    public bool isInWater;
    public bool isInSpacecraft;
}