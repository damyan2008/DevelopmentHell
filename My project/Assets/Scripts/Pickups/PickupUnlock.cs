// UpgradePickup.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class UpgradePickup : MonoBehaviour
{
    [SerializeField] private PlayerAction upgrade = PlayerAction.Jump;
    [SerializeField] private bool destroyOnPickup = true;

    JournalPopUp journalPopUp = new JournalPopUp();

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        
        journalPopUp.AddText(@"The internal connection to the right track movement is binded to 'D';");

        var handler = ProgressionHandler.Instance;
        if (handler == null)
        {
            Debug.LogError("No ProgressionHandler in scene.");
            return;
        }

        bool newlyUnlocked = handler.UnlockUpgrade(upgrade);

        // If you want pickups to disappear even if already unlocked, remove the if.
        if (newlyUnlocked && destroyOnPickup)
            Destroy(gameObject);
    }
}