using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeveloperPanel : MonoBehaviour
{
    public static DeveloperPanel Instance { get; private set; }

    [Header("UI")]
    public GameObject panelRoot;
    public InputField inputCardAmount;
    public InputField inputCardId;
    public Dropdown targetBag;
    public Button btnAddCard;

    private float lastShiftTime = 0f;
    private const float doubleClickInterval = 0.3f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 初始化UI
        if (targetBag != null)
        {
            targetBag.ClearOptions();
            targetBag.AddOptions(new List<string> { "地点", "背包" });
            targetBag.value = 0;
        }
        if (inputCardAmount != null) inputCardAmount.text = "1";
        if (btnAddCard != null) btnAddCard.onClick.AddListener(OnAddClicked);

        if (panelRoot != null) panelRoot.SetActive(false);
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
                // 不要重置lastShiftTime为0，否则只能关闭不能再次打开
            }
            lastShiftTime = now;
        }
    }

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
                card.StartUpdating();
                card.RefreshSlot();
            }
        }
        if (bag?.Window != null) bag.Window.RefreshDisplay();
    }
}
        


