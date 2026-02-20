using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class EyesControlling : MonoBehaviour
{
    private Animator anim;
    private bool isCurrentlyOpen = false;

    [Header("Input Settings")]
    [Tooltip("Drag your .inputactions asset file here")]
    public InputActionAsset inputAsset;
    
    [Tooltip("Type the exact name of the Action inside the 'Eyes' map that uses the P key")]
    public string actionName = "Toggle";

    private InputAction toggleAction;

    void Awake()
    {
        anim = GetComponent<Animator>();

        if (inputAsset != null)
        {
            InputActionMap eyesMap = inputAsset.FindActionMap("Eyes");
            
            if (eyesMap != null)
            {
                toggleAction = eyesMap.FindAction(actionName);
            }
            else
            {
                Debug.LogWarning("Could not find an Action Map named 'Eyes'!");
            }
        }
    }

    void OnEnable()
    {
        if (toggleAction != null)
        {
            toggleAction.Enable(); // Turn the action on
            toggleAction.performed += OnToggleEye; // Listen for the key press
        }
    }

    void OnDisable()
    {
        if (toggleAction != null)
        {
            toggleAction.Disable(); // Turn it off
            toggleAction.performed -= OnToggleEye; // Stop listening
        }
    }

    private void OnToggleEye(InputAction.CallbackContext context)
    {
        isCurrentlyOpen = !isCurrentlyOpen;
        anim.SetBool("IsOpen", isCurrentlyOpen);
    }
}
