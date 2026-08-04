using UnityEngine;

public class AISystemManager : MonoBehaviour
{
    public static AISystemManager Instance { get; private set; }

    public AISystem AI { get; private set; }

    [Header("Supabase")]
    [SerializeField] private KeyData keyData;

    [SerializeField] private string url = "https://twbxebfidwkaukodqcus.supabase.co";
    
    private AIEdgeFunctionClient edgeFunctionClient;

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

        edgeFunctionClient = new AIEdgeFunctionClient(url, keyData.key);
        Debug.Log("edgeFunctionClient 생성");
        AI = new AISystem(edgeFunctionClient);
        Debug.Log("AISystem 생성");
    }
}
