// KeybindUnlockManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class KeybindUnlockManager : MonoBehaviour
{
    public static KeybindUnlockManager Instance { get; private set; }

    /// Fired when an action becomes unlocked.
    public event Action<PlayerAction> OnUnlocked;

    [Header("Defaults (unlocked at new game)")]
    [SerializeField] private List<PlayerAction> unlockedByDefault = new List<PlayerAction> { PlayerAction.Move };

    [Header("Persistence")]
    [SerializeField] private bool usePersistence = true;
    private const string SaveKey = "UnlockedActions_v1";

    private readonly HashSet<PlayerAction> unlocked = new HashSet<PlayerAction>();

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
        unlocked.Clear();

        // Defaults first
        foreach (var a in unlockedByDefault)
            unlocked.Add(a);

        // Then load persistent unlocks (so they override defaults by adding more)
        if (usePersistence)
            Load();
    }

    public bool IsUnlocked(PlayerAction action) => unlocked.Contains(action);

    /// Unlocks and raises event only on first unlock.
    public bool Unlock(PlayerAction action)
    {
        if (unlocked.Contains(action))
            return false;

        unlocked.Add(action);

        if (usePersistence)
            Save();

        OnUnlocked?.Invoke(action);
        return true;
    }

    public void Lock(PlayerAction action)
    {
        if (!unlocked.Remove(action))
            return;

        if (usePersistence)
            Save();
        // Typically you *don't* raise OnUnlocked here; if you want lock events, add another event.
    }

    private void Save()
    {
        // Simple, human-readable serialization: comma-separated enum ints.
        // Example: "0,1,3"
        var values = new List<int>();
        foreach (var a in unlocked)
            values.Add((int)a);

        var data = string.Join(",", values);
        PlayerPrefs.SetString(SaveKey, data);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
            return;

        var data = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrWhiteSpace(data))
            return;

        var parts = data.Split(',');
        foreach (var p in parts)
        {
            if (int.TryParse(p, out int v) && Enum.IsDefined(typeof(PlayerAction), v))
                unlocked.Add((PlayerAction)v);
        }
    }

    // Optional dev convenience
    [ContextMenu("Clear Unlock Save")]
    private void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        Initialize();
    }
}