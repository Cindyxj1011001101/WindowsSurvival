using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameDataManager
{
    private static GameDataManager instance = new();
    public static GameDataManager Instance => instance;

    private GameDataManager()
    {
        // 玩家背包
        playerBagData = JsonManager.LoadData<BagRuntimeData>("PlayerBag");
        // 上次地点
        lastPlace = JsonManager.LoadData<int>("LastPlace");
        // 环境
        environmentBagDataDict = new();
        foreach (PlaceEnum place in Enum.GetValues(typeof(PlaceEnum)))
        {
            environmentBagDataDict.Add(place, JsonManager.LoadData<EnvironmentBagRuntimeData>(place.ToString() + "Bag"));
        }
        // 音频数据
        audioData = JsonManager.LoadData<AudioData>("Audio");
        // 已解锁的配方
        unlockedRecipes = JsonManager.LoadData<List<string>>("UnlockedRecipes");
        // 科技数据
        technologyData = JsonManager.LoadData<TechnologyData>("Technology");
        // 装备数据
        equipmentData = JsonManager.LoadData<BagRuntimeData>("Equipment");
        // 其他数据
        gameRuntimeData = JsonManager.LoadData<GameRuntimeData>("GameRuntimeData");
    }

    #region 玩家背包
    private BagRuntimeData playerBagData;

    public BagRuntimeData PlayerBagData => playerBagData;

    public void SavePlayerBagRuntimeData()
    {
        PlayerBag bag = GameManager.Instance.PlayerBag;
        playerBagData = new();
        playerBagData.cardSlotsRuntimeData = new();
        foreach (var slot in bag.Slots)
        {
            playerBagData.cardSlotsRuntimeData.Add(new() { cardList = slot.Cards });
        }
        JsonManager.SaveData(playerBagData, "PlayerBag");
    }
    #endregion

    #region 地点

    // 最后一次玩家出现时的地点
    private int lastPlace = -1;

    public PlaceEnum LastPlace => (PlaceEnum)lastPlace;

    public void SaveLastPlace()
    {
        JsonManager.SaveData(GameManager.Instance.CurEnvironmentBag.PlaceData.placeType, "LastPlace");
    }
    #endregion

    #region 环境背包

    public Dictionary<PlaceEnum, EnvironmentBagRuntimeData> environmentBagDataDict = new();

    public EnvironmentBagRuntimeData GetEnvironmentBagDataByPlace(PlaceEnum place)
    {
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
            // 保存掉落列表
            data.disposableDropList = bag.disposableDropList;
            data.repeatableDropList = bag.repeatableDropList;
            // 保存背包中的卡牌
            data.cardSlotsRuntimeData = new();
            foreach (var slot in bag.Slots)
            {
                data.cardSlotsRuntimeData.Add(new() { cardList = slot.Cards });
            }
            JsonManager.SaveData(data, place.ToString() + "Bag");
        }
    }
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

    #region 合成
    private List<string> unlockedRecipes;

    public List<string> UnlockedRecipes => unlockedRecipes;

    public void SaveUnlockedRecipes()
    {
        JsonManager.SaveData(unlockedRecipes, "UnlockedRecipes");
    }
    #endregion

    #region 科技
    private TechnologyData technologyData;

    public TechnologyData TechnologyData => technologyData;

    public void SaveTechnologyData()
    {
        JsonManager.SaveData(technologyData, "Technology");
    }
    #endregion

    #region 装备
    private BagRuntimeData equipmentData;
    public BagRuntimeData EquipmentData => equipmentData;

    public void SaveEquipmentData()
    {
        EquipmentBag bag = GameManager.Instance.EquipmentBag;
        equipmentData = new();
        equipmentData.cardSlotsRuntimeData = new()
        {
            new() { cardList = bag.headSlot.Cards },
            new() { cardList = bag.bodySlot.Cards },
            new() { cardList = bag.backSlot.Cards },
            new() { cardList = bag.legSlot.Cards }
        };
        JsonManager.SaveData(equipmentData, "Equipment");
    }
    #endregion

    #region 已生成的对话
    private GeneratedChatData generatedChatData;
    public GeneratedChatData GeneratedChatData => generatedChatData;

    public void SaveGeneratedChatData()
    {
        generatedChatData = new GeneratedChatData();
        generatedChatData.GeneratedChatDataList = ChatManager.Instance.GeneratedChatDataList;
        JsonManager.SaveData(generatedChatData, "GeneratedChatData");
    }

    public void LoadGeneratedChatData()
    {
        generatedChatData = JsonManager.LoadData<GeneratedChatData>("GeneratedChatData");
    }
    #endregion

    #region 游戏运行时数据
    private GameRuntimeData gameRuntimeData;
    public GameRuntimeData GameRuntimeData => gameRuntimeData;

    public void SaveGameRuntimeData()
    {
        JsonManager.SaveData(GameRuntimeData, "GameRuntimeData");
    }
    #endregion
}