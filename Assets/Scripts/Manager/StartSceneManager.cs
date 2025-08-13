using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.IO;

public class StartSceneManager : MonoBehaviour
{
    public GameObject StartButton;
    public GameObject LoadButton;
    public GameObject ChooseSkipGuide;
    private Button EnterGame;
    private Button Setting;
    private Button Exit;

    private Button ReturnStart;

    private Button SkipGuide;
    private Button DontSkipGuide;
    private Button ReturnLoad;
    private void Awake()
    {
        EnterGame = StartButton.transform.Find("EnterGame").GetComponent<Button>();
        Setting = StartButton.transform.Find("Setting").GetComponent<Button>();
        Exit = StartButton.transform.Find("Exit").GetComponent<Button>();
        ReturnStart = LoadButton.transform.Find("ReturnStart").GetComponent<Button>();

        SkipGuide = ChooseSkipGuide.transform.Find("SkipGuide").GetComponent<Button>();
        DontSkipGuide = ChooseSkipGuide.transform.Find("DontSkipGuide").GetComponent<Button>();
        ReturnLoad = ChooseSkipGuide.transform.Find("ReturnLoad").GetComponent<Button>();

        EnterGame.onClick.AddListener(OnEnterGameClick);
        Exit.onClick.AddListener(OnExitClick);
        Setting.onClick.AddListener(OnSettingClick);

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
        int index = int.Parse(name.Substring(name.Length - 1, 1)) - 1;
        // 加载存档
        if (GameDataManager.Instance.LoadData.loads[index] == null)
        {
            LoadButton.SetActive(false);
            ChooseSkipGuide.SetActive(true);
            SkipGuide.onClick.AddListener(() => EnterNewGame(index, true));
            DontSkipGuide.onClick.AddListener(() => EnterNewGame(index, false));
            ReturnLoad.onClick.AddListener(() =>
            {
                ChooseSkipGuide.SetActive(false);
                LoadButton.SetActive(true);
            });
            return;
        }
        else
        {
            //读取存档数据
            GameDataManager.Instance.LoadAllData(index);
            //切换到游戏场景
            SceneManager.LoadScene(1);
        }
    }

    //创建新存档
    public void EnterNewGame(int index, bool skipGuide)
    {
        //创建新存档    
        CreateNewLoad(index, skipGuide);
        GameDataManager.Instance.LoadAllData(index);

        //进入游戏
        SceneManager.LoadScene(1);
    }
    #endregion

    #region 进入游戏按钮事件
    private void OnEnterGameClick()
    {
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
    }
    //删除存档
    public void DeleteLoad(string name)
    {
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
    }

    private void OnSettingClick()
    {
        Debug.Log("Setting");
        //进入设置界面
    }
    private void OnExitClick()
    {
        //发布后退出游戏
        Application.Quit();

        // 在编辑器中停止播放模式
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion

    #region 创建新存档

    //创建新存档(从初始存档位置复制)
    void CreateNewLoad(int Index, bool skipGuide)
    {
        //源路径
        string sourcePath = Path.Combine(Application.streamingAssetsPath, "GameData0");
        //目标路径
        string targetFolder = Application.persistentDataPath + "/GameData" + Index + "/";
        // 如果目标文件夹不存在，先创建
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }
        foreach (string file in Directory.GetFiles(sourcePath, "*.json"))
        {
            File.Copy(file, Path.Combine(targetFolder, Path.GetFileName(file)), true);
        }

        GameDataManager.Instance.CreateNewLoad(Index, skipGuide);
    }

    #endregion
}