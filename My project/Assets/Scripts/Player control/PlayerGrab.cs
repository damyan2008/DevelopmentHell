using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class PlayerGrab : MonoBehaviour
{
    [Header("Grab Check")]
    [SerializeField] private Transform grabPoint;          // usually near hands
    [SerializeField] private float grabRadius = 0.35f;
    [SerializeField] private ItemDropDatabase dropDb;
    [SerializeField] private LayerMask pickupLayerMask;    // set to your Pickup layer

    public void OnGrab(InputAction.CallbackContext ctx)
    {
        var handler = ProgressionHandler.Instance;
        if (handler == null || handler.HasUpgrade(PlayerAction.Grab)) return;

        // Requested behavior:
        // - Drop on key/button PRESS
        // - Pick up on key/button RELEASE

        // PRESS -> drop if holding
        if (ctx.started)
        {
            if (!handler.HasHeldItem) return;

            var held = handler.GetHeldItem();
            if (held.HasValue && dropDb != null)
            {
                var prefab = dropDb.GetPrefab(held.Value);
                if (prefab != null)
                    Instantiate(prefab, grabPoint.position, Quaternion.identity);
            }
            handler.ClearHeldItem();
            return;
        }

        // RELEASE -> pick up if not holding
        if (ctx.canceled)
        {
            if (handler.HasHeldItem) return;

            var hit = Physics2D.OverlapCircle(grabPoint.position, grabRadius, pickupLayerMask);
            if (hit == null) return;

            var pickup = hit.GetComponent<HandItem>();
            if (pickup == null) return;

            bool success = handler.SetHeldItem(pickup.Item, requireEmptyHand: true);
            if (success)
                Destroy(pickup.gameObject);
        }
    }

    // Optional: visualize grab range
    private void OnDrawGizmosSelected()
    {
        if (grabPoint == null) return;
        Gizmos.DrawWireSphere(grabPoint.position, grabRadius);
    }
}
