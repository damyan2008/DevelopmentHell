using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public GameObject player;

    [Tooltip("Optional: auto-find by tag instead of name")]
    [SerializeField] private string playerTag = "Player";

    private void Awake()
    {
        // Singleton guard
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        FindPlayerIfNeeded();
    }

    private void Start()
    {
        // In case Player spawns after Awake
        FindPlayerIfNeeded();
    }

    public void FindPlayerIfNeeded()
    {
        if (player != null) return;

        // Prefer tag lookup (recommended)
        if (!string.IsNullOrEmpty(playerTag))
        {
            player = GameObject.FindGameObjectWithTag(playerTag);
        }

        // Fallback: find by name (if no tag match)
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        if (player == null)
        {
            Debug.LogWarning("GameManager: Player object not found yet.");
        }
    }

    // Call this if your player is spawned later and you want to register it manually
    public void RegisterPlayer(GameObject playerObject)
    {
        player = playerObject;
    }
}