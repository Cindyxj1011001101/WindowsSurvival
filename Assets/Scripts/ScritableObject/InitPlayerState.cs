using UnityEngine;

[CreateAssetMenu(fileName = "InitPlayerStateData", menuName = "ScriptableObject/InitPlayerStateData")]
public class InitPlayerStateData : SingleScriptableObject<InitPlayerStateData>
{
    //玩家状态
    [Header("玩家状态")]
    public float Health;
    public float Fullness;
    public float Thirst;
    public float San;
    public float Oxygen;
    public float Tired;
    //玩家状态基础变化
    [Header("玩家状态基础变化")]
    public float BasicHealthChange;
    public float BasicFullnessChange;
    public float BasicThirstChange;
    public float BasicSanChange;
    public float BasicOxygenChange;
    public float BasicTiredChange;
    //地点状态基础变化
    [Header("地点状态基础变化")]
    public float BasicElectricityChange;
    [Header("初始地点")]
    public PlaceEnum place;
}