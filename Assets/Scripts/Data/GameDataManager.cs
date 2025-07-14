using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameDataManager
{
    private static GameDataManager instance = new();
    public static GameDataManager Instance => instance;

    private GameDataManager() { }

    #region 玩家背包
    private BagRuntimeData playerBagData;

    public BagRuntimeData PlayerBagData
    {
        get
        {
            playerBagData ??= JsonManager.LoadData<BagRuntimeData>("PlayerBag");
            return playerBagData;
        }
    }

    public void SavePlayerBagRuntimeData()
    {
        PlayerBag bag = GameManager.Instance.PlayerBag;
        playerBagData = new();
        //playerBagData.maxLoad = bag.MaxLoad;
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
    private int lastPlace = -1;

    public PlaceEnum LastPlace
    {
        get
        {
            if (lastPlace == -1)
                lastPlace = JsonManager.LoadData<int>("LastPlace");
            return (PlaceEnum)lastPlace;
        }
    }

    public Dictionary<PlaceEnum, EnvironmentBagRuntimeData> environmentBagDataDict = new();

    public EnvironmentBagRuntimeData GetEnvironmentBagDataByPlace(PlaceEnum place)
    {
        if (!environmentBagDataDict.ContainsKey(place))
        {
            var data = JsonManager.LoadData<EnvironmentBagRuntimeData>(place.ToString() + "Bag");
            environmentBagDataDict.Add(place, data);
        }

        return environmentBagDataDict[place];
    }

    /// <summary>
    /// 保存所有环境背包的数据
    /// </summary>
    public void SaveEnvironmentBagRuntimeData()
    {
        foreach (var (place, bag) in GameManager.Instance.EnvironmentBags)
        {
            EnvironmentBagRuntimeData data = new();
            // 保存探索度
            data.discoveryDegree = bag.DiscoveryDegree;
            // 保存一次性掉落列表
            var e = bag.ExploreEvent.eventList.Find(c => c is PlaceDropEvent);
            data.disposableDropList = (e as PlaceDropEvent).curOnceDropList;
            // 保存背包中的卡牌
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
        JsonManager.SaveData(GameManager.Instance.CurEnvironmentBag.PlaceData.placeType, "LastPlace");
    }

    // 动力舱的背包数据
    //private EnvironmentBagRuntimeData poweCabinBagData;
    //public EnvironmentBagRuntimeData PoweCabinBagData => poweCabinBagData;

    //// 驾驶室的背包数据
    //private EnvironmentBagRuntimeData cockpitBagData;
    //public EnvironmentBagRuntimeData CockpitBagData => cockpitBagData;
    #endregion

    #region 音频
    private AudioData audioData;
    public AudioData AudioData
    {
        get
        {
            audioData ??= JsonManager.LoadData<AudioData>("Audio");
            return audioData;
        }
    }

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

    #region 合成
    private List<string> unlockedRecipes;

    public List<string> UnlockedRecipes
    {
        get
        {
            unlockedRecipes ??= JsonManager.LoadData<List<string>>("UnlockedRecipes");
            return unlockedRecipes;
        }
    }

    public void SaveUnlockedRecipes()
    {
        JsonManager.SaveData(unlockedRecipes, "UnlockedRecipes");
    }
    #endregion

    #region 科技
    private TechnologyData technologyData;

    public TechnologyData TechnologyData
    {
        get
        {
            technologyData ??= JsonManager.LoadData<TechnologyData>("Technology");
            return technologyData;
        }
    }

    public void SaveTechnologyData()
    {
        JsonManager.SaveData(technologyData, "Technology");
    }
    #endregion

    #region 装备
    private BagRuntimeData equipmentData;
    public BagRuntimeData EquipmentData
    {
        get
        {
            equipmentData ??= JsonManager.LoadData<BagRuntimeData>("Equipment");
            return equipmentData;
        }
    }

    public void SaveEquipmentData()
    {
        EquipmentBag bag = GameManager.Instance.EquipmentBag;
        equipmentData = new();
        equipmentData.cardSlotsRuntimeData = new();
        foreach (var slot in bag.Slots)
        {
            equipmentData.cardSlotsRuntimeData.Add(new() { cardInstanceList = slot.Cards });
        }
        JsonManager.SaveData(equipmentData, "Equipment");
    }
    #endregion

    #region 游戏运行时数据
    private GameRuntimeData gameRuntimeData;
    public GameRuntimeData GameRuntimeData
    {
        get
        {
            gameRuntimeData ??= JsonManager.LoadData<GameRuntimeData>("GameRuntimeData");
            return gameRuntimeData;
        }
    }

    public void SaveGameRuntimeData()
    {
        JsonManager.SaveData(GameRuntimeData, "GameRuntimeData");
    }
    #endregion
}