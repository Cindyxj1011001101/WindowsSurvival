using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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


}