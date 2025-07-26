using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;

public class StartSceneManager : MonoBehaviour
{
    public GameObject StartButton;
    public GameObject LoadButton;
    private Button EnterGame;
    private Button Setting;
    private Button Exit;
    private void Awake()
    {
        EnterGame = StartButton.transform.Find("EnterGame").GetComponent<Button>();
        Setting = StartButton.transform.Find("Setting").GetComponent<Button>();
        Exit = StartButton.transform.Find("Exit").GetComponent<Button>();

        EnterGame.onClick.AddListener(OnEnterGameClick);
        Exit.onClick.AddListener(OnExitClick);
        Setting.onClick.AddListener(OnSettingClick);

        StartButton.SetActive(true);
        LoadButton.SetActive(false);
    }


    #region 存档按钮刷新    
    private void RefreshLoadButton()
    {
        //显示现在的存档情况
        for (int i = 0; i < LoadButton.transform.childCount; i++)
        {
            GameObject button = LoadButton.transform.GetChild(i).gameObject;
            //显示存档名（存档1，存档2，存档3，存档4，无）
            if (GameDataManager.Instance.LoadData.loads[i] != null && GameDataManager.Instance.LoadData.loads[i].GameTime != DateTime.MinValue)
            {
                button.transform.Find("Name").GetComponent<Text>().text = "存档" + (i + 1);
                //显示存档时间
                DateTime now = GameDataManager.Instance.LoadData.loads[i].GameTime;
                DateTime target = new DateTime(2020, 1, 1, 0, 0, 0);
                TimeSpan span = target - now;
                int days = span.Days;
                int hours = now.Hour;
                int minutes = now.Minute;
                button.transform.Find("Time").GetComponent<Text>().text = days + "天" + hours.ToString("D2") + ":" + minutes.ToString("D2");
            }
            else
            {
                button.transform.Find("Name").GetComponent<Text>().text = "（空）";
                button.transform.Find("Time").GetComponent<Text>().text = "00:00";
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
            Debug.Log("存档不存在");
            //创建新存档    
            CreateNewLoad(index);
            GameDataManager.Instance.LoadAllData(index);
            //进入游戏
            SceneManager.LoadScene(1);
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
    #endregion

    #region 进入游戏按钮事件
    private void OnEnterGameClick()
    {
        //进入存档选择界面
        StartButton.SetActive(false);
        LoadButton.SetActive(true);

        GameDataManager.Instance.LoadLoadData();
        //显示现在的存档情况
        RefreshLoadButton();
        //添加按钮事件
        for (int i = 0; i < LoadButton.transform.childCount; i++)
        {
            GameObject button = LoadButton.transform.GetChild(i).gameObject;
            string btnName = button.name; // 局部变量
            button.GetComponent<Button>().onClick.AddListener(() => ClickLoad(btnName));
        }
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


    //创建新存档(从初始存档位置复制)
    void CreateNewLoad(int Index)
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
        Debug.Log("文件已复制到: " + targetFolder);
    }



}