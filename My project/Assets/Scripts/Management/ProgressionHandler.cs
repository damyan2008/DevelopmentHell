// ProgressionHandler.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerAction
{    
    Move,
    Jump,
    Crouch,
    Grab,
    Oven,
    MoveLeft,
    MoveRight
    
}

public enum ItemId
{
    Rock,
    Bomb,
    Keycard
}

public class ProgressionHandler : MonoBehaviour
{
    public static ProgressionHandler Instance { get; private set; }

    public event Action<PlayerAction> OnUpgradeUnlocked;
    public event Action<ItemId?> OnHeldItemChanged;

    private ItemId? heldItem = null;

    public bool HasHeldItem => heldItem.HasValue;
    public ItemId? GetHeldItem() => heldItem;

    private readonly HashSet<PlayerAction> unlockedUpgrades = new();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- Upgrades ---
    public bool HasUpgrade(PlayerAction id) => unlockedUpgrades.Contains(id);

    public bool UnlockUpgrade(PlayerAction id)
    {
        if (!unlockedUpgrades.Add(id))
            return false;

        OnUpgradeUnlocked?.Invoke(id);
        return true;
    }

    // --- Inventory ---
    public bool SetHeldItem(ItemId id, bool requireEmptyHand = true)
    {
        if (requireEmptyHand && heldItem.HasValue) return false;

        heldItem = id;
        OnHeldItemChanged?.Invoke(heldItem);
        return true;
    }

    public void ClearHeldItem()
    {
        heldItem = null;
        OnHeldItemChanged?.Invoke(heldItem);
    }

    public bool IsHolding(ItemId id) => heldItem.HasValue && heldItem.Value == id;
}