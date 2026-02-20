using UnityEngine;

public class HandItem : MonoBehaviour
{
    [SerializeField] private ItemId item;
    public ItemId Item => item;
}