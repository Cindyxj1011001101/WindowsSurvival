using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DeveloperPanel : MonoBehaviour
{
    public static DeveloperPanel Instance { get; private set; }

    // 显示用的中文映射
    private static readonly Dictionary<PlaceEnum, string> PlaceDisplayNames = new()
    {
        { PlaceEnum.PowerCabin, "动力舱" },
        { PlaceEnum.Cockpit, "驾驶室" },
        { PlaceEnum.LifeSupportCabin, "维生舱" },
        { PlaceEnum.CoralCoast, "珊瑚礁海域" },
        { PlaceEnum.PhosphorTomb, "织光藻墓园" },
        { PlaceEnum.SpaceshipOuterHull, "飞船外壳" },
        { PlaceEnum.ShallowGrotto, "浅层岩穴" },
        { PlaceEnum.VictimsHall, "遇难者大厅" },
        { PlaceEnum.LastSanctuary, "最后庇护所" },
    };

    private static readonly Dictionary<PlayerStateEnum, string> StateDisplayNames = new()
    {
        { PlayerStateEnum.Health, "健康" },
        { PlayerStateEnum.Hunger, "饱食" },
        { PlayerStateEnum.Hydration, "水分" },
        { PlayerStateEnum.Sanity, "精神" },
        { PlayerStateEnum.Oxygen, "氧气" },
        { PlayerStateEnum.Sobriety, "清醒度" },
        { PlayerStateEnum.Load, "负重" },
        { PlayerStateEnum.COPoisoning, "一氧化碳中毒" },
        { PlayerStateEnum.Itchiness, "瘙痒" },
        { PlayerStateEnum.PainLevel, "疼痛" },
        { PlayerStateEnum.BodyTemperature, "体温" },
    };

    [Header("第一行，卡牌添加相关UI")]
    public GameObject panelRoot;
    public InputField inputCardAmount;
    public InputField inputCardId;
    public Dropdown targetBag;
    public Button btnAddCard;

    [Header("第二行，玩家状态控制相关UI")]
    public Dropdown stateDropdown;      // 玩家状态枚举
    public Dropdown opDropdown;         // + 或 -
    public InputField inputStateValue;  // 数值
    public Button btnApplyState;        // 应用按钮
    [Header("第三行，开发者移动相关UI")] 
    public Dropdown placeDropdown;      // 用于选择目的地点
    public Button btnMoveToPlace;       // 开发者面板直接移动（不创建回程卡）

    [Header("其他控制UI")]
    public Button btnAddStudyProcess;   // 研究进度增加按钮
    
    public Button btnUnlockAllTechnologies; // 研究全解锁按钮
    
    

    

    private float lastShiftTime = 0f;
    private const float doubleClickInterval = 0.3f;
    // 下拉索引到 PlaceEnum 的映射，避免依赖下拉文本解析
    private List<PlaceEnum> placeOptions = new();
    // 下拉索引到 PlayerStateEnum 的映射
    private List<PlayerStateEnum> stateOptions = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitCardAddUI();
        InitPlayerStateUI();
        InitOtherUI();
        InitDevMoveUI();

        if (panelRoot != null) panelRoot.SetActive(false);
    }

    /// <summary>
    /// 初始化卡牌添加相关UI
    /// </summary>
    private void InitCardAddUI()
    {
        if (targetBag != null)
        {
            targetBag.ClearOptions();
            targetBag.AddOptions(new List<string> { "地点", "背包" });
            targetBag.value = 0;
        }
        if (inputCardAmount != null) inputCardAmount.text = "1";
        if (inputCardId != null) inputCardId.text = "压缩饼干";
        if (btnAddCard != null) btnAddCard.onClick.AddListener(OnAddClicked);
    }

    /// <summary>
    /// 初始化玩家状态控制相关UI
    /// </summary>
    private void InitPlayerStateUI()
    {
        if (opDropdown != null)
        {
            opDropdown.ClearOptions();
            opDropdown.AddOptions(new List<string> { "+", "-" });
            opDropdown.value = 0;
        }
        if (stateDropdown != null)
        {
            stateDropdown.ClearOptions();
            stateOptions.Clear();

            var options = new List<string>();
            foreach (PlayerStateEnum state in Enum.GetValues(typeof(PlayerStateEnum)))
            {
                stateOptions.Add(state);
                options.Add(GetStateDisplayName(state));
            }

            stateDropdown.AddOptions(options);
            stateDropdown.value = stateOptions.IndexOf(PlayerStateEnum.Hunger); // 默认选饱食
        }
        if (inputStateValue != null) inputStateValue.text = "10";
        if (btnApplyState != null) btnApplyState.onClick.AddListener(OnApplyStateClicked);
    }
    /// <summary>
    /// 初始化其他控制UI
    /// </summary>
    private void InitOtherUI()
    {
        if (btnAddStudyProcess != null) btnAddStudyProcess.onClick.AddListener(OnApplyAddStudyProcess);
    }

    /// <summary>
    /// 初始化开发者移动与快捷操作UI
    /// </summary>
    private void InitDevMoveUI()
    {
        if (placeDropdown != null)
        {
            placeDropdown.ClearOptions();
            placeOptions.Clear();
            var names = new List<string>();

            if (GameManager.Instance != null && GameManager.Instance.PlaceDataDict != null && GameManager.Instance.PlaceDataDict.Count > 0)
            {
                foreach (var kv in GameManager.Instance.PlaceDataDict)
                {
                    placeOptions.Add(kv.Key);

                    names.Add(GetPlaceDisplayName(kv.Key));
                }
            }
            else
            {
                // 作为后备，使用枚举名
                foreach (var n in Enum.GetNames(typeof(PlaceEnum)))
                {
                    if (Enum.TryParse<PlaceEnum>(n, out var e))
                    {
                        placeOptions.Add(e);
                        names.Add(GetPlaceDisplayName(e));
                    }
                }
            }

            placeDropdown.AddOptions(names);
            placeDropdown.value = 0;
        }

        if (btnMoveToPlace != null)
        {
            btnMoveToPlace.onClick.AddListener(OnDevMoveToPlaceClicked);
        }

        if (btnUnlockAllTechnologies != null)
        {
            // 绑定“研究全解锁”按钮，在 Inspector 中确保已关联该按钮
            btnUnlockAllTechnologies.onClick.AddListener(OnUnlockAllTechnologiesClicked);
        }
    }

    

    private void OnDevMoveToPlaceClicked()
    {
        // 确认在 Inspector 中已绑定 `placeDropdown`
        if (placeDropdown == null) return;

        // 使用 placeOptions 映射获取枚举值，避免依赖显示文本解析
        if (placeOptions != null && placeOptions.Count > placeDropdown.value)
        {
            var target = placeOptions[placeDropdown.value];
            GameManager.Instance.ChangeEnv(target, false);
            return;
        }

        // 后备：尝试解析下拉文本为枚举（保持向后兼容）
        var name = placeDropdown.options[placeDropdown.value].text;
        if (!Enum.TryParse<PlaceEnum>(name, true, out var parsedTarget)) return;
        GameManager.Instance.ChangeEnv(parsedTarget, false);
    }

    private void OnUnlockAllTechnologiesClicked()
    {
        // 调用 TechnologyManager 的开发者方法，立即解锁所有科技及其配方
        TechnologyManager.Instance.UnlockAllTechnologies();
    }

    private void Update()
    {
        // 双击Shift打开/关闭面板
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            float now = Time.unscaledTime;
            if (now - lastShiftTime < doubleClickInterval)
            {
                if (panelRoot != null) panelRoot.SetActive(!panelRoot.activeSelf);
            }
            lastShiftTime = now;
        }
    }

    /// <summary>
    /// 添加卡牌到指定背包
    /// </summary>
    private void OnAddClicked()
    {
        int amount = 1;
        int.TryParse(inputCardAmount.text, out amount);
        if (amount < 1) amount = 1;
        string cardId = inputCardId.text.Trim();
        if (string.IsNullOrEmpty(cardId)) return;

        string target = targetBag.options[targetBag.value].text;
        Bag bag = null;

        if (target == "背包")
        {
            // 自动打开玩家背包窗口
            WindowsManager.Instance.OpenWindow("PlayerBag");
            var playerBagWindow = FindObjectOfType<PlayerBagWindow>();
            if (playerBagWindow != null && playerBagWindow.Bag != null)
                bag = playerBagWindow.Bag;
        }
        else // 地点
        {
            // 自动打开环境背包窗口
            WindowsManager.Instance.OpenWindow("EnvironmentBag");
            var envBagWindow = FindObjectOfType<EnvironmentBagWindow>();
            if (envBagWindow != null && envBagWindow.Bag != null)
                bag = envBagWindow.Bag;
        }

        for (int i = 0; i < amount; i++)
        {
            var card = CardFactory.CreateCard(cardId);
            if (bag != null && bag.CanAddCard(card, out _))
            {
                bag.AddCard(card);
                card.Init();
                card.RefreshSlot();
            }
        }
        if (bag?.Window != null) bag.Window.RefreshDisplay();
    }

    /// <summary>
    /// 应用玩家状态变更
    /// </summary>
    private void OnApplyStateClicked()
    {
        if (stateDropdown == null || inputStateValue == null || opDropdown == null) return;

        // 使用预缓存的枚举列表，显示名可中文，逻辑依旧基于枚举值
        if (stateOptions == null || stateDropdown.value < 0 || stateDropdown.value >= stateOptions.Count) return;
        var stateEnum = stateOptions[stateDropdown.value];

        float value = 10;
        float.TryParse(inputStateValue.text, out value);
        if (opDropdown.value == 1) value = -value; // 1为“-”

        StateManager.Instance.ChangePlayerState(stateEnum, value);
    }
    private void OnApplyAddStudyProcess()
    {
        if (TechnologyManager.Instance.CurStudiedTechNode != null)
        {
            TechnologyManager.Instance.AddStudyProgress(99999999); // 研究进度增加
        }
        
    }

    private string GetPlaceDisplayName(PlaceEnum place)
    {
        return PlaceDisplayNames.TryGetValue(place, out var name) ? name : place.ToString();
    }

    private string GetStateDisplayName(PlayerStateEnum state)
    {
        return StateDisplayNames.TryGetValue(state, out var name) ? name : state.ToString();
    }
    
}



