// InventoryItemPickup.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InventoryItemPickup : MonoBehaviour
{
    [SerializeField] private ItemId item = ItemId.Rock;
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private bool requireEmptyHand = true;
    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var handler = ProgressionHandler.Instance;
        if (handler == null) return;

        bool success = handler.SetHeldItem(item, requireEmptyHand);
        if (success && destroyOnPickup) Destroy(gameObject);
    }
}