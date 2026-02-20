// ItemDropDatabase.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Items/Drop Database")]
public class ItemDropDatabase : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public ItemId itemId;
        public GameObject pickupPrefab; // prefab that has HandPickup + collider, etc.
    }

    [SerializeField] private Entry[] entries;

    public GameObject GetPrefab(ItemId id)
    {
        foreach (var e in entries)
            if (e.itemId == id) return e.pickupPrefab;
        return null;
    }
}