using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatUIController : MonoBehaviour
{
    [SerializeField] private OpenAICompatibleLLMClient llmClient;
    [SerializeField] private InputField inputField;
    [SerializeField] private TMP_InputField tmpInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private Text messagePrefab;
    [SerializeField] private TMP_Text tmpMessagePrefab;
    [SerializeField] private string systemPrompt = "You are a helpful assistant.";

    private bool isSending;

    private void Awake()
    {
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendClicked);
        }
    }

    public void OnSendClicked()
    {
        if (isSending)
        {
            return;
        }

        if (llmClient == null || contentRoot == null)
        {
            Debug.LogError("ChatUIController is missing references.");
            return;
        }

        string userMessage = GetInputText();
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            return;
        }

        AddMessage("You: " + userMessage);
        SetInputText(string.Empty);

        isSending = true;
        if (sendButton != null)
        {
            sendButton.interactable = false;
        }

        llmClient.SendUserMessage(
            systemPrompt,
            userMessage,
            onSuccess: reply =>
            {
                AddMessage("AI: " + reply);
                SetSending(false);
            },
            onError: error =>
            {
                AddMessage("Error: " + error);
                SetSending(false);
            }
        );
    }

    private void AddMessage(string text)
    {
        if (messagePrefab != null)
        {
            Text item = Instantiate(messagePrefab, contentRoot);
            item.text = text;
            return;
        }

        if (tmpMessagePrefab != null)
        {
            TMP_Text item = Instantiate(tmpMessagePrefab, contentRoot);
            item.text = text;
            return;
        }

        Debug.LogError("ChatUIController is missing message prefab.");
    }

    private void SetSending(bool sending)
    {
        isSending = sending;
        if (sendButton != null)
        {
            sendButton.interactable = !sending;
        }
    }

    private string GetInputText()
    {
        if (inputField != null)
        {
            return inputField.text;
        }

        if (tmpInputField != null)
        {
            return tmpInputField.text;
        }

        Debug.LogError("ChatUIController is missing input field.");
        return string.Empty;
    }

    private void SetInputText(string text)
    {
        if (inputField != null)
        {
            inputField.text = text;
        }

        if (tmpInputField != null)
        {
            tmpInputField.text = text;
        }
    }
}
