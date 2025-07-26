using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameDataManager
{
    private static GameDataManager instance = new();
    public static GameDataManager Instance => instance;

    public int curLoadIndex; // 当前存档索引

    public string CurLoadName => "GameData" + curLoadIndex.ToString(); // 当前存档名称

    private GameDataManager()
    {
        curLoadIndex = 0;
        // 玩家背包
        playerBagData = JsonManager.LoadData<BagRuntimeData>(CurLoadName, "PlayerBag");
        // 上次地点
        lastPlace = JsonManager.LoadData<int>(CurLoadName, "LastPlace");
        // 环境
        environmentBagDataDict = new();
        foreach (PlaceEnum place in Enum.GetValues(typeof(PlaceEnum)))
        {
            environmentBagDataDict.Add(place, JsonManager.LoadData<EnvironmentBagRuntimeData>(CurLoadName, place.ToString() + "Bag"));
        }
        // 状态数据
        stateData = JsonManager.LoadData<StateData>(CurLoadName, "State");
        // 音频数据
        audioData = JsonManager.LoadData<AudioData>(CurLoadName, "Audio");
        // 已解锁的配方
        unlockedRecipes = JsonManager.LoadData<List<string>>(CurLoadName, "UnlockedRecipes");
        // 科技数据
        technologyData = JsonManager.LoadData<TechnologyData>(CurLoadName, "Technology");
        // 装备数据
        equipmentData = JsonManager.LoadData<BagRuntimeData>(CurLoadName, "Equipment");
        // 已生成的对话
        generatedChatData = JsonManager.LoadData<GeneratedChatData>(CurLoadName, "GeneratedChatData");
        // 其他数据
        timeData = JsonManager.LoadData<TimeData>(CurLoadName, "TimeData");
    }

    public void LoadAllData(int index)
    {
        curLoadIndex = index;
        loadData.loads[index] = new Load(new DateTime(2020, 1, 1, 0, 0, 0));
        // 玩家背包
        playerBagData = JsonManager.LoadData<BagRuntimeData>(CurLoadName, "PlayerBag");
        // 上次地点
        lastPlace = JsonManager.LoadData<int>(CurLoadName, "LastPlace");
        // 环境
        environmentBagDataDict = new();
        foreach (PlaceEnum place in Enum.GetValues(typeof(PlaceEnum)))
        {
            environmentBagDataDict.Add(place, JsonManager.LoadData<EnvironmentBagRuntimeData>(CurLoadName, place.ToString() + "Bag"));
        }
        // 状态数据
        stateData = JsonManager.LoadData<StateData>(CurLoadName, "State");
        // 音频数据
        audioData = JsonManager.LoadData<AudioData>(CurLoadName, "Audio");
        // 已解锁的配方
        unlockedRecipes = JsonManager.LoadData<List<string>>(CurLoadName, "UnlockedRecipes");
        // 科技数据
        technologyData = JsonManager.LoadData<TechnologyData>(CurLoadName, "Technology");
        // 装备数据
        equipmentData = JsonManager.LoadData<BagRuntimeData>(CurLoadName, "Equipment");
        // 已生成的对话
        generatedChatData = JsonManager.LoadData<GeneratedChatData>(CurLoadName, "GeneratedChatData");
        // 时间数据
        timeData = JsonManager.LoadData<TimeData>(CurLoadName, "TimeData");
    }

    public void SaveAllData()
    {
        // 玩家背包
        SavePlayerBagRuntimeData();
        // 上次地点
        SaveLastPlace();
        // 环境
        SaveEnvironmentBagRuntimeData();
        // 状态
        SaveStateData();
        // 音频数据
        SaveAudioData();
        // 已解锁的配方
        SaveUnlockedRecipes();
        // 科技数据
        SaveTechnologyData();
        // 装备数据
        SaveEquipmentData();
        // 已生成的对话
        SaveGeneratedChatData();
        // 时间数据
        SaveTimeData();

        if (loadData == null)
        {
            loadData = new LoadData();
            for (int i = 0; i < loadData.loads.Length; i++)
            {
                loadData.loads[i] = new Load();
            }
        }
        // 保存时间
        loadData.loads[curLoadIndex].GameTime = timeData.curTime;
        // 保存存档数据
        SaveLoadData();
        SceneManager.LoadScene(0);
    }

    #region 存档数据
    private LoadData loadData;

    public LoadData LoadData => loadData;
    public void SaveLoadData()
    {
        JsonManager.SaveData(loadData, "LoadData", "LoadData");
    }
    public void LoadLoadData()
    {
        loadData = JsonManager.LoadData<LoadData>("LoadData", "LoadData");
    }
    #endregion

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
        JsonManager.SaveData(playerBagData, CurLoadName, "PlayerBag");
    }

    public void LoadPlayerBagRuntimeData()
    {
        playerBagData = JsonManager.LoadData<BagRuntimeData>(CurLoadName, "PlayerBag");
    }
    #endregion

    #region 地点

    // 最后一次玩家出现时的地点
    private int lastPlace = -1;

    public PlaceEnum LastPlace => (PlaceEnum)lastPlace;

    public void SaveLastPlace()
    {
        JsonManager.SaveData(GameManager.Instance.CurEnvironmentBag.PlaceData.placeType, CurLoadName, "LastPlace");
    }

    public void LoadLastPlace()
    {
        lastPlace = JsonManager.LoadData<int>(CurLoadName, "LastPlace");
    }
    #endregion

    #region 环境背包

    public Dictionary<PlaceEnum, EnvironmentBagRuntimeData> environmentBagDataDict = new();

    public EnvironmentBagRuntimeData GetEnvironmentBagDataByPlace(PlaceEnum place)
    {
        return environmentBagDataDict[place];
    }

    public void LoadEnvironmentBagRuntimeData()
    {
        environmentBagDataDict = new();
        foreach (PlaceEnum place in Enum.GetValues(typeof(PlaceEnum)))
        {
            environmentBagDataDict.Add(place, JsonManager.LoadData<EnvironmentBagRuntimeData>(CurLoadName, place.ToString() + "Bag"));
        }
    }

    /// <summary>
    /// 保存所有环境背包的数据
    /// </summary>
    public void SaveEnvironmentBagRuntimeData()
    {
        foreach (var (place, bag) in GameManager.Instance.EnvironmentBags)
        {
            EnvironmentBagRuntimeData data = new();
            data.init = true;
            // 保存掉落列表
            data.disposableDropList = bag.DisposableDropList;
            data.repeatableDropList = bag.RepeatableDropList;
            // 保存背包中的卡牌
            data.cardSlotsRuntimeData = new();
            // 保存铺设电缆状态
            data.hasCable = bag.HasCable;
            // 保存压强状态
            data.pressureLevel = bag.PressureLevel;
            // 保存其他状态
            data.environmentStateDict = bag.StateDict;
            foreach (var slot in bag.Slots)
            {
                data.cardSlotsRuntimeData.Add(new() { cardList = slot.Cards });
            }
            JsonManager.SaveData(data, CurLoadName, place.ToString() + "Bag");
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
        JsonManager.SaveData(audioData, CurLoadName, "Audio");
    }

    public void LoadAudioData()
    {
        audioData = JsonManager.LoadData<AudioData>(CurLoadName, "Audio");
    }

    #endregion

    #region 合成
    private List<string> unlockedRecipes;

    public List<string> UnlockedRecipes => unlockedRecipes;

    public void SaveUnlockedRecipes()
    {
        JsonManager.SaveData(unlockedRecipes, CurLoadName, "UnlockedRecipes");
    }

    public void LoadUnlockedRecipes()
    {
        unlockedRecipes = JsonManager.LoadData<List<string>>(CurLoadName, "UnlockedRecipes");
    }
    #endregion

    #region 科技
    private TechnologyData technologyData;

    public TechnologyData TechnologyData => technologyData;

    public void SaveTechnologyData()
    {
        JsonManager.SaveData(technologyData, CurLoadName, "Technology");
    }

    public void LoadTechnologyData()
    {
        technologyData = JsonManager.LoadData<TechnologyData>(CurLoadName, "Technology");
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
        JsonManager.SaveData(equipmentData, CurLoadName, "Equipment");
    }

    public void LoadEquipmentData()
    {
        equipmentData = JsonManager.LoadData<BagRuntimeData>(CurLoadName, "Equipment");
    }
    #endregion

    #region 已生成的对话
    private GeneratedChatData generatedChatData;
    public GeneratedChatData GeneratedChatData => generatedChatData;

    public void SaveGeneratedChatData()
    {
        generatedChatData.GeneratedChatDataList = new List<ChatData>(ChatManager.Instance.GeneratedChatDataList);
        JsonManager.SaveData(generatedChatData, CurLoadName, "GeneratedChatData");
    }

    public void LoadGeneratedChatData()
    {
        generatedChatData = JsonManager.LoadData<GeneratedChatData>(CurLoadName, "GeneratedChatData");
        ChatManager.Instance.GeneratedChatDataList = generatedChatData.GeneratedChatDataList;
    }
    #endregion

    #region 游戏时间数据
    private TimeData timeData;
    public TimeData TimeData => timeData;

    public void SaveTimeData()
    {
        timeData.init = true;
        timeData.curTime = TimeManager.Instance.curTime;
        timeData.curIntervel = TimeManager.Instance.curInterval;
        JsonManager.SaveData(timeData, CurLoadName, "TimeData");
    }

    public void LoadGameRuntimeData()
    {
        timeData = JsonManager.LoadData<TimeData>(CurLoadName, "TimeData");
    }
    #endregion

    #region 状态数据
    private StateData stateData;
    public StateData StateData => stateData;

    public void LoadStateData()
    {
        stateData = JsonManager.LoadData<StateData>(CurLoadName, "State");
    }

    public void SaveStateData()
    {
        stateData = new StateData
        {
            init = true,
            electricity = StateManager.Instance.Electricity,
            waterLevel = StateManager.Instance.WaterLevel,
            playerState = StateManager.Instance.PlayerStateDict,
            maxLoad = StateManager.Instance.MaxLoad,
            curLoad = StateManager.Instance.CurLoad
        };
        JsonManager.SaveData(stateData, CurLoadName, "State");
    }
    #endregion
}