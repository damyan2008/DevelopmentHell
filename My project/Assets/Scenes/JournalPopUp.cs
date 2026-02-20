using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class JournalPopUp : MonoBehaviour
{
    public GameObject journalPanel;
    public TMPro.TMP_InputField journalInputField; 
    private bool isJournalOpen = false;

    public Animator journalAnimator; 

    // 1. ADDED THIS BACK: This ensures the journal only toggles ONCE per key press
    public void OnJournalToggle(InputAction.CallbackContext context)
    {
        // Only fire when the key is pressed down, ignore the release
        if (context.performed)
        {
            ToggleJournal();
        }
    }

    public void ToggleJournal() // Note: I removed the 'public' here if you only call it via the context method above, but kept it public just in case.
    {
        isJournalOpen = !isJournalOpen;

        if (isJournalOpen)
        {
            journalPanel.SetActive(true);
            
            if (journalAnimator != null)
            {
                journalAnimator.SetTrigger("Open");
            }
            //yield return new WaitForSecondsRealtime(0.5f);
            journalInputField.ActivateInputField();
            Time.timeScale = 0f;
        }
        else
        {
            if (journalAnimator != null)
            {
                // 2. FIXED TYPO HERE: Changed "Open" to "Close"
                journalAnimator.SetTrigger("Close");
            }
            else
            {
                Debug.LogError("The Animator is missing from the Inspector slot!");
            }

            journalInputField.text = journalInputField.text; 
            journalInputField.DeactivateInputField();
            journalPanel.SetActive(false);
            
            //StartCoroutine(DisableAfterAnimation());
        }
    }

    private System.Collections.IEnumerator DisableAfterAnimation()
    {
        // Adjust this 0.5f to match exactly how long your close animation takes
        yield return new WaitForSecondsRealtime(0.5f);

        if (!isJournalOpen) 
        {
            journalPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}