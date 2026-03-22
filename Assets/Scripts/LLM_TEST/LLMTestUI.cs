using UnityEngine;

public class LLMTestUI : MonoBehaviour
{
    [SerializeField] private OpenAICompatibleLLMClient llmClient;

    public void TestSend()
    {
        string systemPrompt = "You are a gentle fantasy RPG village NPC.";
        string userMessage = "Say a welcome line to a player who just arrived.";

        llmClient.SendUserMessage(
            systemPrompt,
            userMessage,
            onSuccess: reply =>
            {
                Debug.Log("LLM reply: " + reply);
            },
            onError: error =>
            {
                Debug.LogError(error);
            }
        );
    }
}
