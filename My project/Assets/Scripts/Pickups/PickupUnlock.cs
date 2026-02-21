// UpgradePickup.cs
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class UpgradePickup : MonoBehaviour
{
    [SerializeField] private PlayerAction upgrade = PlayerAction.Jump;
    [SerializeField] private bool destroyOnPickup = true;

    public TextMeshProUGUI journalTextDisplay;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;


        if (upgrade == PlayerAction.MoveLeft)
        {

            journalTextDisplay.text += "\n" + "- " + @"The internal connection to the right track movement is binded to 'D'...:D";
        }
        else if (upgrade == PlayerAction.Oven)
        {
             journalTextDisplay.text += "\n" + @"Good job, you are the best chicken making robot!! That is mayby why they thied to kill you.";
        }
        else if (upgrade == PlayerAction.Jump)
        {
            journalTextDisplay.text += "\n" + @"M - jump (hold for a while)";
        }
        //Debug.Log("NESTO");

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