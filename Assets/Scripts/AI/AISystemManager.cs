using UnityEngine;

public class AISystemManager : MonoBehaviour
{
    public static AISystemManager Instance { get; private set; }

    public AISystem AI { get; private set; }

    [Header("OpenAI")]
    [SerializeField]
    private string model = "gpt-5-mini";
    [SerializeField] private KeyData keyData;
    [SerializeField] private KeyData inputValidationInstruction;
    [SerializeField] private KeyData detectiveInstruction;
    [SerializeField] private KeyData evaluationInstruction;
    
    private OpenAIClient openAIClient;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    private void Initialize()
    {
        if (keyData == null || string.IsNullOrEmpty(keyData.key)) return;
        string apiKey = keyData.key;

        openAIClient = new OpenAIClient(apiKey);

        AI = new AISystem(
            openAIClient,
            model,
            inputValidationInstruction.key,
            detectiveInstruction.key,
            evaluationInstruction.key);
    }
}
