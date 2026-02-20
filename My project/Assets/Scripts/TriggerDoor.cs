using UnityEngine;

public class TriggerDoor : MonoBehaviour
{
    [SerializeField] private PressureButton button;
    [SerializeField] private Animator animator;

    [Header("Animator params")]
    [SerializeField] private string openTrigger = "Open";
    [SerializeField] private string closeTrigger = "Close";

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (!button) return;
        button.Pressed += HandlePressed;
        button.Released += HandleReleased;

        // Optional: sync state when enabling
        if (button.IsPressed) HandlePressed();
        else HandleReleased();
    }

    private void OnDisable()
    {
        if (!button) return;
        button.Pressed -= HandlePressed;
        button.Released -= HandleReleased;
    }

    private void HandlePressed()
    {
        if (animator) animator.SetTrigger(openTrigger);
    }

    private void HandleReleased()
    {
        if (animator) animator.SetTrigger(closeTrigger);
    }
}