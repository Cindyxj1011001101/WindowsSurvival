using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadButton : MonoBehaviour
{
    public void LoadScene()
    {
        GameDataManager.Instance.SaveAllData();
        SceneManager.LoadScene(0);
    }
}