using UnityEngine;

public class ConveyorConnection : MonoBehaviour
{
    [Tooltip("True if this connection receives items into its inventory. False if it provides items out.")]
    [SerializeField] private bool isReceiver;

    [Tooltip("If set, this connection will only provide this specific item type.")]
    [SerializeField] private ItemSO craftedOut;

    private Inventory inventory;
    public LineRenderer lr;
    public bool inUse = false;


//How about belts also have conveyer connections. That way everything interacts with eachother in the same way and i can use it to make merge and splits?
    void Start()
    {
        inventory = GetComponentInParent<Inventory>();
        if (inventory == null)
        {
            Debug.LogError($"ConveyorConnection on {gameObject.name}: No Inventory found in parent.");
            enabled = false;
            return;
        }

        if (lr != null)
        {
            lr.positionCount = 1;
            lr.SetPosition(0, transform.position);
        }
    }

    public bool IsReceiver => isReceiver;

    /// Called by a belt that wants to deliver an item to this connection.
    public bool TryReceive(ItemSO item)
    {
        if (!isReceiver) return false;
        if (inventory.GetCount(item) >= item.maxStack) return false;

        inventory.AddItem(item);
        return true;
    }

    /// Called by a belt that wants to pull an item from this connection.
    public ItemSO TryProvide()
    {
        if (isReceiver) return null;

        ItemSO item = craftedOut != null ? craftedOut : inventory.GetFirstItem();
        if (item == null) return null;
        if (inventory.IsEmpty(item)) return null;

        inventory.RemoveItem(item);
        return item;
    }
}
