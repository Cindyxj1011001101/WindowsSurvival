using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonEvent : MonoBehaviour
{
    public void LoadScene()
    {
        GameDataManager.Instance.SaveAllData();
        SceneManager.LoadScene(0);
    }

    public void Sleep()
    {
        StateManager.Instance.Sleep();
    }

    public void Die()
    {
        int index = GameDataManager.Instance.curLoadIndex;
        //删除本存档
        Debug.Log(GameDataManager.Instance.LoadData);
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
        //返回初始界面
        SceneManager.LoadScene(0);
    }
}