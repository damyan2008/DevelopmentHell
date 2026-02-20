using System.Collections;
using UnityEngine;

public class EyeOpenWipe : MonoBehaviour
{
    [Header("Assign the RectTransform that has RectMask2D")]
    public RectTransform revealWindow;

    public float duration = 1.0f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Coroutine routine;
    void Start() => PlayOpen();

    void OnEnable()
    {
        // Start closed
        Vector2 size = revealWindow.sizeDelta;
        revealWindow.sizeDelta = new Vector2(0f, size.y);
    }

    public void PlayOpen()
    {
        Debug.Log("s");
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        // Full canvas width (safe for overlay UI)
        float targetWidth = ((RectTransform)revealWindow.parent).rect.width;

        float t = 0f;
        float startWidth = revealWindow.sizeDelta.x;
        float height = revealWindow.sizeDelta.y;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float k = ease.Evaluate(Mathf.Clamp01(t));
            float w = Mathf.Lerp(startWidth, targetWidth, k);
            revealWindow.sizeDelta = new Vector2(w, height);
            yield return null;
        }

        revealWindow.sizeDelta = new Vector2(targetWidth, height);
    }
}