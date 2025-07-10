using System.Collections.Generic;
using System.IO;

public enum CSVEnum
{
    Chat,//对话
}

public enum Character
{
    Player,
    NPC,
}

public class ChatData
{
    public int SentenceId;
    public int ParagraphId;
    public Character Character;
    public bool Choosable;
    public string Content;
    public int NextSentenceId;
    public ChatData(string[] line)
    {
        SentenceId = int.Parse(line[0]);
        ParagraphId = int.Parse(line[1]);
        Character = (Character)int.Parse(line[2]);
        Choosable = bool.Parse(line[3]);
        Content = line[4];
    }
}

/// <summary>
/// 读取CSV文件
/// </summary>
public class CSVReader
{
    /// <summary>
    /// CSV文件夹路径
    /// </summary>
    public static string FolderPath = "Assets/Resources/CSV";

    /// <summary>
    /// 读取CSV文件
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>CSV文件内容</returns>
    public static List<string[]> ReadCSVByFileName( string fileName)
    {
        var result = new List<string[]>();
        string filePath = Path.Combine(FolderPath, fileName);
        if (File.Exists(filePath))
        {
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var values = line.Split(',');
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
    // public static List<T> ReadCSVByEnum(CSVEnum csvEnum, string fileName)
    // {
    //     List<string[]> lines = ReadCSVByFileName(fileName);
    //     switch (csvEnum)
    //     {
    //         case CSVEnum.Chat:
    //             return CharCSVResolve(lines);
    //         default:
    //             return null;
    //     }
    // }

    private static List<ChatData> CharCSVResolve(List<string[]> lines)
    {
        List<ChatData> result = new List<ChatData>();
        foreach (var line in lines)
        {
            result.Add(new ChatData(line));
        }
        return result;
    }
}