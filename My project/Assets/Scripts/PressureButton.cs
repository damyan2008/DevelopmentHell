using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class PressureButton : MonoBehaviour
{
    public enum ButtonMode { Normal, Heavy }

    [Header("Mode")]
    [SerializeField] private ButtonMode mode = ButtonMode.Normal;

    [Tooltip("Heavy mode: how long the button 'blips' when only a pickup lands on it.")]
    [SerializeField] private float pulseDuration = 0.15f;

    [Header("What can press the button?")]
    [Tooltip("Layers that count as pickups/weights (non-player pressers).")]
    [SerializeField] private int playerLayer;
    [SerializeField] private int pickupLayer;

    [Tooltip("If true, colliders tagged Player count as the player presser.")]
    [SerializeField] private bool allowPlayerTag = true;

    [SerializeField] private string playerTag = "Player";

    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string pressTrigger = "Press";
    [SerializeField] private string releaseTrigger = "Release";

    [Header("State (read-only)")]
    [field: SerializeField] public bool IsPressed { get; private set; }

    [Header("Events")]
    public UnityEvent onPressed;
    public UnityEvent onReleased;
    public event Action Pressed;
    public event Action Released;

    // For Normal mode we only need a count.
    private int normalPressCount = 0;

    // For Heavy mode we track exactly "player present?" and "pickup present?"
    private int playerCount = 0;
    private int pickupCount = 0;
    
    private bool pulseActive = false;
    private Coroutine pulseRoutine;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isPlayer = IsPlayer(other);
        bool isPickup = IsPickup(other);

        if (mode == ButtonMode.Normal)
        {
            if (!isPlayer && !isPickup) return;
            normalPressCount++;
            SetPressed(true);
            return;
        }

        // Heavy mode
        if (!isPlayer && !isPickup) return;

        bool pickupWasZero = pickupCount == 0;
        bool playerWasZero = playerCount == 0;
        if (isPlayer) playerCount++;
        if (isPickup) pickupCount++;

        // Held condition: both present
        if (playerCount > 0 && pickupCount > 0)
        {
            StopPulseIfRunning();
            Debug.Log(isPickup + " " + isPlayer);
            SetPressed(true);
            return;
        }

        // Pulse condition: pickup just landed without player
        // (i.e., we saw a transition from no-pickup -> pickup, and player is not on it)
        if ((isPickup && pickupWasZero && playerCount == 0) || (isPlayer && playerWasZero && pickupCount == 0))
        {
            StartPickupPulse();
        }

        RecomputePressed();
        // If player enters alone (no pickup), do nothing in Heavy mode.
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        bool isPlayer = IsPlayer(other);
        bool isPickup = IsPickup(other);

        if (mode == ButtonMode.Normal)
        {
            if (!isPlayer && !isPickup) return;
            normalPressCount = Mathf.Max(0, normalPressCount - 1);
            if (normalPressCount == 0) SetPressed(false);
            return;
        }

        // Heavy mode
        if (!isPlayer && !isPickup) return;

        if (isPlayer) playerCount = Mathf.Max(0, playerCount - 1);
        if (isPickup) pickupCount = Mathf.Max(0, pickupCount - 1);
        RecomputePressed();

    }

    private bool IsPlayer(Collider2D col) => col.gameObject.layer == playerLayer;
    private bool IsPickup(Collider2D col) => col.gameObject.layer == pickupLayer;

    private void StartPickupPulse()
    {
        Debug.Log("RAGHH");
        StopPulseIfRunning();
        pulseRoutine = StartCoroutine(PulseCoroutine());
    }

    private void StopPulseIfRunning()
    {
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }
    }

    private IEnumerator PulseCoroutine()
    {
        pulseActive = true;
        RecomputePressed();

        float t = 0f;
        while (t < pulseDuration)
        {
            if (playerCount > 0 && pickupCount > 0)
            {
                pulseActive = false;
                pulseRoutine = null;
                RecomputePressed(); // remains pressed because held
                yield break;
            }
            t += Time.deltaTime;
            yield return null;
        }

        pulseActive = false;
        pulseRoutine = null;
        Debug.Log("RAGHH");
        RecomputePressed(); // releases unless held
    }

    private void SetPressed(bool pressed)
    {
        
        if (pressed == IsPressed) return;

        IsPressed = pressed;

        if (IsPressed)
        {
            if (animator) animator.SetTrigger(pressTrigger);
            onPressed?.Invoke();
            Pressed?.Invoke();
        }
        else
        {
            if (animator) animator.SetTrigger(releaseTrigger);
            onReleased?.Invoke();
            Released?.Invoke();
        }
    }
    private void RecomputePressed()
    {
        bool shouldBePressed;

        if (mode == ButtonMode.Normal)
            shouldBePressed = normalPressCount > 0;
        else
            shouldBePressed = (playerCount > 0 && pickupCount > 0) || pulseActive;

        SetPressed(shouldBePressed);
    }
}