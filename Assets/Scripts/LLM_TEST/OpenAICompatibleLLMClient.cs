using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LLMStructuredResult
{
    public string reply;
    public string summary;
}

public class OpenAICompatibleLLMClient : MonoBehaviour
{
    [SerializeField] private LLMSettings settings = new LLMSettings();

    public void SendUserMessage(string systemPrompt, string userMessage, Action<string> onSuccess, Action<string> onError)
    {
        SendStructuredUserMessage(
            backgroundAndIdentity: systemPrompt,
            previousSummary: "无",
            systemInfo: "无",
            userMessage: userMessage,
            onSuccess: result => onSuccess?.Invoke(result.reply),
            onError: onError
        );
    }

    public void SendStructuredUserMessage(
        string backgroundAndIdentity,
        string previousSummary,
        string systemInfo,
        string userMessage,
        Action<LLMStructuredResult> onSuccess,
        Action<string> onError)
    {
        StartCoroutine(SendStructuredRequestCoroutine(backgroundAndIdentity, previousSummary, systemInfo, userMessage, onSuccess, onError));
    }

    private IEnumerator SendStructuredRequestCoroutine(
        string backgroundAndIdentity,
        string previousSummary,
        string systemInfo,
        string userMessage,
        Action<LLMStructuredResult> onSuccess,
        Action<string> onError)
    {
        string url = settings.baseUrl.TrimEnd('/') + "/chat/completions";

        if (string.IsNullOrEmpty(backgroundAndIdentity)) backgroundAndIdentity = "你是游戏内助手。";
        previousSummary = string.IsNullOrWhiteSpace(previousSummary) ? "无" : previousSummary;
        systemInfo = string.IsNullOrWhiteSpace(systemInfo) ? "无" : systemInfo;

        string formatInstruction =
            "你必须按以下格式输出，不要输出其他标题：\n" +
            "[REPLY]\n" +
            "<给玩家的回复>\n" +
            "[SUMMARY]\n" +
            "<更新后的前文概括，简洁但覆盖关键事实与关系>";

        ChatRequest requestObj = new ChatRequest
        {
            model = settings.model,
            temperature = settings.temperature,
            max_tokens = settings.maxTokens,
            stream = false,
            messages = new ChatMessage[]
            {
                new ChatMessage { role = "system", content = "【背景、身份设定】\n" + backgroundAndIdentity },
                new ChatMessage { role = "system", content = "【前文概括】\n" + previousSummary },
                new ChatMessage { role = "system", content = "【系统信息】\n" + systemInfo },
                new ChatMessage { role = "system", content = formatInstruction },
                new ChatMessage { role = "user", content = userMessage }
            }
        };

        string json = JsonUtility.ToJson(requestObj);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 30;

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + settings.apiKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke("Request failed: " + request.error + "\n" + request.downloadHandler.text);
                yield break;
            }

            string responseText = request.downloadHandler.text;
            Debug.Log("Raw Response: " + responseText);

            ChatResponse responseObj = JsonUtility.FromJson<ChatResponse>(responseText);

            if (responseObj != null &&
                responseObj.choices != null &&
                responseObj.choices.Length > 0 &&
                responseObj.choices[0].message != null)
            {
                string content = responseObj.choices[0].message.content;
                var parsed = ParseStructuredResponse(content, previousSummary);
                onSuccess?.Invoke(parsed);
            }
            else
            {
                onError?.Invoke("Parse failed: " + responseText);
            }
        }
    }

    public LLMSettings GetSettings()
    {
        return settings;
    }

    public void SetSettings(LLMSettings newSettings)
    {
        settings = newSettings;
    }

    private LLMStructuredResult ParseStructuredResponse(string content, string fallbackSummary)
    {
        var result = new LLMStructuredResult
        {
            reply = content ?? string.Empty,
            summary = string.IsNullOrWhiteSpace(fallbackSummary) ? "无" : fallbackSummary
        };

        if (string.IsNullOrEmpty(content)) return result;

        int replyTag = content.IndexOf("[REPLY]", StringComparison.OrdinalIgnoreCase);
        int summaryTag = content.IndexOf("[SUMMARY]", StringComparison.OrdinalIgnoreCase);

        if (replyTag >= 0 && summaryTag >= 0)
        {
            int replyStart = replyTag + "[REPLY]".Length;
            if (summaryTag > replyStart)
            {
                result.reply = content.Substring(replyStart, summaryTag - replyStart).Trim();
                int summaryStart = summaryTag + "[SUMMARY]".Length;
                if (summaryStart <= content.Length)
                    result.summary = content.Substring(summaryStart).Trim();
                return result;
            }
        }

        // 模型未严格遵循格式时，保留全文为回复，摘要沿用旧值
        return result;
    }
}

