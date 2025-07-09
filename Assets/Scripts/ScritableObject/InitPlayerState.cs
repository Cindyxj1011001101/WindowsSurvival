using UnityEngine;

[CreateAssetMenu(fileName = "InitPlayerStateData", menuName = "ScritableObject/InitPlayerStateData")]
public class InitPlayerStateData:SingleScriptableObject<InitPlayerStateData>
{
    public int Health;
    public int Fullness;
    public int Thirst;
    public int Tired;
    public int San;
    public float BasicHealthChange;
    public float BasicFullnessChange;
    public float BasicThirstChange;
    public float BasicTiredChange;
    public float BasicSanChange;
    public int maxPlayerGrid;
    public float maxPlayerWeight;
    public float maxWeightFactor;
    public PlaceEnum place;
}