using System;

[Serializable]
public class LLMSettings
{
    public string baseUrl = "https://api.deepseek.com";
    public string apiKey = "";
    public string model = "deepseek-chat";
    public float temperature = 0.7f;
    public int maxTokens = 256;
}
