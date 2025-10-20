using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Collections;

public class StartSceneManager : MonoBehaviour
{
    public GameObject StartButton;
    public GameObject LoadButton;
    public GameObject ChooseSkipGuide;
    private Button EnterGame;
    private Button Setting;
    private Button Exit;

    private bool isButtonCooldown = false;
    private float buttonCooldownTime =0.5f; // 0.5秒冷却时间
    
    private Button ReturnStart;

    private Button ClearData;

    private Button SkipGuide;
    private Button DontSkipGuide;
    private Button ReturnLoad;
    private void Awake()
    {
        EnterGame = StartButton.transform.Find("EnterGame").GetComponent<Button>();
        Setting = StartButton.transform.Find("Setting").GetComponent<Button>();
        Exit = StartButton.transform.Find("Exit").GetComponent<Button>();
        
        ReturnStart = LoadButton.transform.Find("ReturnStart").GetComponent<Button>();
        ClearData = LoadButton.transform.Find("ClearData").GetComponent<Button>();
        
        SkipGuide = ChooseSkipGuide.transform.Find("SkipGuide").GetComponent<Button>();
        DontSkipGuide = ChooseSkipGuide.transform.Find("DontSkipGuide").GetComponent<Button>();
        ReturnLoad = ChooseSkipGuide.transform.Find("ReturnLoad").GetComponent<Button>();

        EnterGame.onClick.AddListener(OnEnterGameClick);
        Exit.onClick.AddListener(OnExitClick);
        Setting.onClick.AddListener(OnSettingClick);
        ClearData.onClick.AddListener(OnClearDataClicked);
        
        StartButton.SetActive(true);
        LoadButton.SetActive(false);
        ChooseSkipGuide.SetActive(false);
    }


    #region 存档按钮刷新    
    private void RefreshLoadButton()
    {
        //显示现在的存档情况
        for (int i = 0; i < 4; i++)
        {
            GameObject button = LoadButton.transform.GetChild(i).gameObject;
            //显示存档名（存档1，存档2，存档3，存档4，无）
            if (GameDataManager.Instance.LoadData.loads[i] != null && GameDataManager.Instance.LoadData.loads[i].GameTime != DateTime.MinValue)
            {
                button.transform.GetChild(0).transform.Find("Name").GetComponent<Text>().text = "存档" + (i + 1);
                //显示存档时间
                DateTime now = GameDataManager.Instance.LoadData.loads[i].GameTime;
                DateTime target = new DateTime(2020, 1, 1, 0, 0, 0);
                TimeSpan span = now - target;
                int days = span.Days;
                int hours = now.Hour;
                int minutes = now.Minute;
                button.transform.GetChild(0).transform.Find("Time").GetComponent<Text>().text = days + "天" + hours.ToString("D2") + ":" + minutes.ToString("D2");
                button.transform.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
                string btnName = button.name; // 局部变量
                button.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => ClickLoad(btnName));
                button.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => DeleteLoad(btnName));
            }
            else
            {
                button.transform.GetChild(0).transform.Find("Name").GetComponent<Text>().text = "（空）";
                button.transform.GetChild(0).transform.Find("Time").GetComponent<Text>().text = "00:00";
                string btnName = button.name; // 局部变量
                button.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => ClickLoad(btnName));
                button.transform.GetChild(1).gameObject.SetActive(false);
            }
        }
    }
    #endregion

    #region 存档按钮事件
    //加载存档
    private void ClickLoad(string name)
    {
        if (isButtonCooldown) return;
        isButtonCooldown = true;
        int index = int.Parse(name.Substring(name.Length - 1, 1)) - 1;
        // 加载存档
        if (GameDataManager.Instance.LoadData.loads[index] == null)
        {
            LoadButton.SetActive(false);
            ChooseSkipGuide.SetActive(true);
            SkipGuide.onClick.AddListener(() =>
            {
                if (isButtonCooldown) return;
                isButtonCooldown = true;
                CreateNewLoad(index, true);
                LoadGameScene(index);
                StartCoroutine(ResetCooldown());
            });
            DontSkipGuide.onClick.AddListener(() =>
            {
                if (isButtonCooldown) return;
                isButtonCooldown = true;
                CreateNewLoad(index, false);
                LoadGameScene(index);
                StartCoroutine(ResetCooldown());
            });
            ReturnLoad.onClick.AddListener(() =>
            {
                ChooseSkipGuide.SetActive(false);
                LoadButton.SetActive(true);
            });
        }
        else
        {
            LoadGameScene(index);
        }
        StartCoroutine(ResetCooldown());
    }
    
    
    private IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(buttonCooldownTime);
        isButtonCooldown = false;
    }
    

    // 载入游戏场景
    public void LoadGameScene(int index)
    {
        GameDataManager.Instance.LoadAllData(index);
        //进入游戏
        MySceneManager.LoadScene(1);
    }
    #endregion

    #region 进入游戏按钮事件
    private void OnEnterGameClick()
    {
        if (isButtonCooldown) return;
        isButtonCooldown = true;
        //进入存档选择界面
        StartButton.SetActive(false);
        LoadButton.SetActive(true);
        ReturnStart.onClick.AddListener(() =>
        {
            StartButton.SetActive(true);
            LoadButton.SetActive(false);
        });
        // 读取存档数据
        GameDataManager.Instance.LoadLoadData();
        //显示现在的存档情况
        RefreshLoadButton();
        //添加按钮事件
        StartCoroutine(ResetCooldown());
    }
    //删除存档
    public void DeleteLoad(string name)
    {
        if (isButtonCooldown) return;
        isButtonCooldown = true;
        int index = int.Parse(name.Substring(name.Length - 1, 1)) - 1;
        GameDataManager.Instance.LoadData.loads[index] = null;
        GameDataManager.Instance.SaveLoadData();
        //目标路径
        string targetFolder = Application.persistentDataPath + "/GameData" + index + "/";
        // 如果目标文件夹不存在，先创建
        if (Directory.Exists(targetFolder))
        {
            Directory.Delete(targetFolder, true);
        }
        else
        {
            Debug.Log("存档不存在");
            return;
        }
        //刷新存档按钮
        RefreshLoadButton();
        StartCoroutine(ResetCooldown());
    }

    private void OnSettingClick()
    {
        if (isButtonCooldown) return;
        isButtonCooldown = true;
        Debug.Log("Setting");
        //进入设置界面
        StartCoroutine(ResetCooldown());
    }
    private void OnExitClick()
    {
        if (isButtonCooldown) return;
        isButtonCooldown = true;
        StartCoroutine(ResetCooldown());
        //发布后退出游戏
        Application.Quit();

        // 在编辑器中停止播放模式
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        
    }

    private void OnClearDataClicked()
    {
        // 如果目标文件夹不存在，先创建
        string targetFolder = Application.persistentDataPath+"/"+"Unity";
        if (Directory.Exists(targetFolder))
        {
            Directory.Delete(targetFolder, true);
        }
        targetFolder = Application.persistentDataPath + "/" + "LoadData";
        if (Directory.Exists(targetFolder))
        {
            Directory.Delete(targetFolder, true);
        }
        for (int i = 0; i < 4; i++)
        {
            targetFolder = Application.persistentDataPath + "/" + "GameData"+i.ToString();
            if (Directory.Exists(targetFolder))
            {
                Directory.Delete(targetFolder, true);
            }
        }
        //刷新存档按钮
        GameDataManager.Instance.ClearLoadData();
        RefreshLoadButton();

    }

    #endregion
    
    #region 创建新存档
    //创建新存档(从初始存档位置复制)
    void CreateNewLoad(int Index, bool skipGuide)
    {
        //目标路径
        string targetFolder = Application.persistentDataPath + "/GameData" + Index + "/";
        // 如果目标文件夹不存在，先创建
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }
        GameDataManager.Instance.CreateNewLoad(Index, skipGuide);
    }
    #endregion
}