using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DeveloperPanel : MonoBehaviour
{
    public static DeveloperPanel Instance { get; private set; }

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

    [Header("其他控制UI")]
    public Button btnAddStudyProcess;   // 研究进度增加按钮

    private float lastShiftTime = 0f;
    private const float doubleClickInterval = 0.3f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitCardAddUI();
        InitPlayerStateUI();
        InitOtherUI();

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
            var names = Enum.GetNames(typeof(PlayerStateEnum));
            stateDropdown.AddOptions(new List<string>(names));
            stateDropdown.value = Array.IndexOf(names, "Fullness");
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
                card.Init();
                bag.AddCard(card);
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

        string stateName = stateDropdown.options[stateDropdown.value].text;
        if (!Enum.TryParse<PlayerStateEnum>(stateName, out var stateEnum)) return;

        float value = 10;
        float.TryParse(inputStateValue.text, out value);
        if (opDropdown.value == 1) value = -value; // 1为“-”

        StateManager.Instance.ChangePlayerState(stateEnum, value);
    }
    private void OnApplyAddStudyProcess()
    {
        if (TechnologyManager.Instance.CurStudiedTechNode != null)
        {
            TechnologyManager.Instance.AddStudyProcess(99999999); // 研究进度增加
        }
        
    }
    
}



