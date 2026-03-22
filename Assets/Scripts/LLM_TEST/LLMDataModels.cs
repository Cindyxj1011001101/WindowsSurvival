using System;

[Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}

[Serializable]
public class ChatRequest
{
    public string model;
    public ChatMessage[] messages;
    public float temperature;
    public int max_tokens;
    public bool stream = false;
}

[Serializable]
public class ChatResponse
{
    public Choice[] choices;
}

[Serializable]
public class Choice
{
    public AssistantMessage message;
}

[Serializable]
public class AssistantMessage
{
    public string role;
    public string content;
}
