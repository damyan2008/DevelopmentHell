
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class JournalPopUp : MonoBehaviour
{
    public GameObject journalPanel;
    public TMP_InputField journalInputField; 
    private bool isJournalOpen = false;

    public void OnJournalToggle(InputAction.CallbackContext context)
    {
        // Only fire when the key is pressed down
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
            // 1. Show the panel
            journalPanel.SetActive(true);
            
            // 2. Select the text box for instant typing
            journalInputField.ActivateInputField();
            
            // 3. Pause the game
            Time.timeScale = 0f; 
        }
        else
        {
            // 1. Force the text box to 'save' its current string if you clicked away
            journalInputField.text = journalInputField.text; 
            
            // 2. Stop the blinking cursor and exit "edit mode"
            journalInputField.DeactivateInputField();
            
            // 3. Hide the panel
            journalPanel.SetActive(false);
            
            // 4. UN-PAUSE THE GAME (This is what you were missing!)
            Time.timeScale = 1f; 
        }
    }
}
