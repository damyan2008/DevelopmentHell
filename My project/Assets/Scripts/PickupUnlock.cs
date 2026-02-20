// UnlockPickup.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class UnlockPickup : MonoBehaviour
{
    [SerializeField] private PlayerAction unlocksAction = PlayerAction.Jump;
    [SerializeField] private bool destroyOnPickup = true;

    private void Reset()
    {
        // Ensure trigger for pickup behavior
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        // Keep it simple: tag check. Swap for a Player component check if you prefer.
        if (!other.CompareTag("Player"))
            return;

        var mgr = KeybindUnlockManager.Instance;
        if (mgr == null)
        {
            Debug.LogError("No KeybindUnlockManager in scene.");
            return;
        }
        Debug.Log("Coll");

        mgr.Unlock(unlocksAction);
        Debug.Log("Jumpy");

        if (destroyOnPickup)
            Destroy(gameObject);
    }
}