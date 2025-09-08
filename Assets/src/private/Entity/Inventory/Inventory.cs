using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Inventory : MonoBehaviour
{
    [Header("Storage Settings")]
    [Tooltip("Items that this inventory accepts. Leave empty to allow all.")]
    public List<ItemSO> allowedItems = new List<ItemSO>();
    protected Dictionary<ItemSO, int> items = new Dictionary<ItemSO, int>();

    private void AddItem(ItemSO item, int count = 1)
    {
        if (allowedItems.Count > 0 && !allowedItems.Contains(item))
        {
            Debug.LogWarning($"{item.name} is not allowed in {gameObject.name}'s inventory.");
            return;
        }

        if (items.ContainsKey(item))
            items[item] += count;
        else
            items[item] = count;

        if (items[item] <= 0)
            items.Remove(item);


        OnInventoryChanged();
    }

    private bool RemoveItem(ItemSO item, int count = 1)
    {
        if (!items.ContainsKey(item) || items[item] < count)
            return false;

        items[item] -= count;
        if (items[item] <= 0)
            items.Remove(item);

        OnInventoryChanged();
        return true;
    }

    public int GetCount(ItemSO item) =>
        items.TryGetValue(item, out int count) ? count : 0;

    public IReadOnlyDictionary<ItemSO, int> GetAllItems() => items;

    public void GiveItemToOther(ItemSO item, int amount, Inventory other)
    {

        // other may reject the item
        if (other.allowedItems.Count > 0 && !other.allowedItems.Contains(item))
            return;

        int myCount = GetCount(item);
        int theirCount = other.GetCount(item);

        // How many I can actually give
        int canGive = Mathf.Min(myCount, amount);

        // How much space the other inventory has
        int canReceive = item.maxStack - theirCount;

        // Final transfer amount = min of both
        int transferAmount = Mathf.Min(canGive, canReceive);

        if (transferAmount <= 0) return;

        // Apply transfer
        RemoveItem(item, transferAmount);
        other.AddItem(item, transferAmount);
        

    }

    // Force subclasses to define their own behavior if needed
    public abstract void OnInventoryChanged();
}
