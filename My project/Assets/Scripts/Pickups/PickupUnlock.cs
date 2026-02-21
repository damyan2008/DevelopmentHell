// UpgradePickup.cs
using TMPro;
using UnityEngine;
using TMPro;

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
        else
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

        //if(other.gameObject.layer == LayerMask.NameToLayer("journal"))
        //{
        //    if(other == null)
        //{
       //     return;
       // }
       // if(journalPanel == null)
       // {
       //     return;
       // }
       // if(journalTextDisplay == null)
       // {
       //     return;
       // }
           // journalPanel.SetActive(true);
           // journalTextDisplay.text += "Use M to jump!";
        //}

        bool newlyUnlocked = handler.UnlockUpgrade(upgrade);

        // If you want pickups to disappear even if already unlocked, remove the if.
        if (newlyUnlocked && destroyOnPickup)
            Destroy(gameObject);
    }
}