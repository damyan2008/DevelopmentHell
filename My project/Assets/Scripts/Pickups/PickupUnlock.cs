// UpgradePickup.cs
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class UpgradePickup : MonoBehaviour
{
    [SerializeField] private PlayerAction upgrade = PlayerAction.Jump;
    [SerializeField] private bool destroyOnPickup = true;

   // [SerializeField] private TextMeshProUGUI journalTextDisplay;
   // [SerializeField] private GameObject journalPanel;

    //[SerializeField] private JournalPopUp journalPopUp;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
    

        //journalPopUp.AddText(@"The internal connection to the right track movement is binded to 'D';");

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