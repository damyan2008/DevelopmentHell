
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class JournalPopUp : MonoBehaviour
{
   public GameObject journalPanel;
    // 1. CHANGED: Now uses the Text component instead of an Input Field
    public TextMeshProUGUI journalTextDisplay; 
    
    // 2. ADDED: Reference to handle the Action Map switch for the Esc key
    public PlayerInput playerInput;

    private bool isJournalOpen = false;

    public void OnJournalToggle(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleJournal();
        }
    }

    public void ToggleJournal() 
    {
        isJournalOpen = !isJournalOpen;

        if (isJournalOpen)
        {
            journalPanel.SetActive(true);
            Time.timeScale = 0f; 

            // 3. SWITCH MAP: This makes the "Esc" key in your Journal map start working
            if (playerInput != null) 
                playerInput.SwitchCurrentActionMap("Journal");
        }
        else
        {
            journalPanel.SetActive(false);
            Time.timeScale = 1f; 

            // 4. SWITCH BACK: This enables your Player map again
            if (playerInput != null) 
                playerInput.SwitchCurrentActionMap("Player");
        }
    }

    // --- 5. THE "ON COMMAND" FUNCTION ---
    // Call this from other scripts or buttons to add text
    public void AddText(string newEntry)
    {
        if (journalTextDisplay != null)
        {
            journalTextDisplay.text += "\n" + "- " + newEntry;
        }
    }
}