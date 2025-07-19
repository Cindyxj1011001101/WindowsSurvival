using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 读取CSV文件
/// </summary>
public class CSVReader
{
    /// <summary>
    /// CSV文件夹路径
    /// </summary>
    public static string FolderPath = "Assets/Resources/CSV/";

    /// <summary>
    /// 读取CSV文件
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>CSV文件内容</returns>
    public static List<string[]> ReadCSVByFileName( string fileName)
    {
        var result = new List<string[]>();
        string filePath = FolderPath+fileName+".csv";
        if (File.Exists(filePath))
        {
            var lines = File.ReadAllLines(filePath);
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');
                result.Add(values);
            }
        }
        return result;
    }

    /// <summary>
    /// 读取CSV文件
    /// </summary>
    /// <param name="csvEnum">CSV枚举</param>
    /// <param name="fileName">文件名</param>
    /// <returns>CSV文件内容</returns>
    public static void ReadChatData(string fileName)
    {
        List<string[]> lines = ReadCSVByFileName(fileName);
        int paragraphIndex = 1;
        List<ChatData> chatDataList = new List<ChatData>();
        List<ParagraphData> result = new List<ParagraphData>();
        for (int i = 0; i < lines.Count; i++)
        {  
            if(lines[i][1] == paragraphIndex.ToString())
            {
                chatDataList.Add(new ChatData(lines[i]));
            }
            else
            {
                result.Add(new ParagraphData(paragraphIndex, chatDataList,lines[i][5]));
                paragraphIndex++;
                chatDataList = new List<ChatData>();
                chatDataList.Add(new ChatData(lines[i]));
            }
        }
        ChatManager.Instance.ParagraphDataList = result;
    }
}