using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameDataManager
{
    private static GameDataManager instance = new();
    public static GameDataManager Instance => instance;

    public GameDataManager()
    {
        lastPlace = JsonManager.LoadData<PlaceEnum>("LastPlace");

        playerBagData = JsonManager.LoadData<PlayerBagRuntimeData>("PlayerBag");

        poweCabinBagData = JsonManager.LoadData<EnvironmentBagRuntimeData>("PowerCabinBag");
        environmentBagDataDict.Add(PlaceEnum.PowerCabin, poweCabinBagData);

        cockpitBagData = JsonManager.LoadData<EnvironmentBagRuntimeData>("CockpitBag");
        environmentBagDataDict.Add(PlaceEnum.Cockpit, cockpitBagData);

        audioData = JsonManager.LoadData<AudioData>("Audio");
    }

    #region 玩家背包
    private PlayerBagRuntimeData playerBagData;

    public PlayerBagRuntimeData PlayerBagData => playerBagData;

    public void SavePlayerBagRuntimeData()
    {
        PlayerBag bag = EffectResolve.Instance.PlayerBag;
        playerBagData = new();
        playerBagData.maxLoad = bag.MaxLoad;
        playerBagData.cardSlotsRuntimeData = new();
        foreach (var slot in bag.Slots)
        {
            playerBagData.cardSlotsRuntimeData.Add(new() { cardInstanceList = slot.Cards });
        }
        JsonManager.SaveData(playerBagData, "PlayerBag");
    }
    #endregion

    #region 环境背包
    // 最后一次玩家出现时的地点
    private PlaceEnum lastPlace;

    public PlaceEnum LastPlace => lastPlace;

    public Dictionary<PlaceEnum, EnvironmentBagRuntimeData> environmentBagDataDict = new();

    public EnvironmentBagRuntimeData GetEnvironmentBagDataByPlace(PlaceEnum place) => environmentBagDataDict[place];

    /// <summary>
    /// 保存所有环境背包的数据
    /// </summary>
    public void SaveEnvironmentBagRuntimeData()
    {
        foreach (var (place, bag) in EffectResolve.Instance.EnvironmentBags)
        {
            EnvironmentBagRuntimeData data = new();
            //data.place = bag.Place;
            data.discoveryDegree = bag.DiscoveryDegree;
            data.cardSlotsRuntimeData = new();
            foreach (var slot in bag.Slots)
            {
                data.cardSlotsRuntimeData.Add(new() { cardInstanceList = slot.Cards });
            }
            JsonManager.SaveData(data, place.ToString() + "Bag");
        }
    }

    public void SaveLastPlace()
    {
        JsonManager.SaveData(EffectResolve.Instance.CurEnvironmentBag.PlaceData.placeType, "LastPlace");
    }

    // 动力舱的背包数据
    private EnvironmentBagRuntimeData poweCabinBagData;
    public EnvironmentBagRuntimeData PoweCabinBagData => poweCabinBagData;

    // 驾驶室的背包数据
    private EnvironmentBagRuntimeData cockpitBagData;
    public EnvironmentBagRuntimeData CockpitBagData => cockpitBagData;
    #endregion

    #region 音频
    private AudioData audioData;
    public AudioData AudioData => audioData;

    public UnityEvent onBGMVolumeChanged = new();

    public void SetMasterVolume(float volume)
    {
        audioData.masterVolume = Mathf.Clamp01(volume);
        onBGMVolumeChanged?.Invoke();
    }
    public void SetBGMVolume(float volume)
    {
        audioData.bgmVolume = Mathf.Clamp01(volume);
        onBGMVolumeChanged?.Invoke();
    }
    public void SetSFXVolume(float volume)
    {
        audioData.sfxVolume = Mathf.Clamp01(volume);
    }

    public void SaveAudioData()
    {
        JsonManager.SaveData(audioData, "Audio");
    }

    #endregion
}