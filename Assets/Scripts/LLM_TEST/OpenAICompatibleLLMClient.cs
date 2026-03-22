using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class OpenAICompatibleLLMClient : MonoBehaviour
{
    [SerializeField] private LLMSettings settings = new LLMSettings();

    public void SendUserMessage(string systemPrompt, string userMessage, Action<string> onSuccess, Action<string> onError)
    {
        StartCoroutine(SendRequestCoroutine(systemPrompt, userMessage, onSuccess, onError));
    }

    private IEnumerator SendRequestCoroutine(string systemPrompt, string userMessage, Action<string> onSuccess, Action<string> onError)
    {
        string url = settings.baseUrl.TrimEnd('/') + "/chat/completions";

        ChatRequest requestObj = new ChatRequest
        {
            model = settings.model,
            temperature = settings.temperature,
            max_tokens = settings.maxTokens,
            stream = false,
            messages = new ChatMessage[]
            {
                new ChatMessage { role = "system", content = systemPrompt },
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
                onSuccess?.Invoke(content);
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
}
