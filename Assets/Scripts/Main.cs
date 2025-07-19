using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        //Debug.Log(Application.persistentDataPath);
        CSVReader.ReadChatData("test");
    }
}
