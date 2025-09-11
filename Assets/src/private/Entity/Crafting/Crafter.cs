using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crafter : MonoBehaviour
{
    private Inventory inventory;

    public ItemSO craft;

    void Start()
    {
        inventory = GetComponent<Inventory>();
    }

    void FixedUpdate()
    {
        CraftItem(craft);
    }

    public void CraftItem(ItemSO craft)
    {
        var recipe = craft.GetCraftingRecipe();
        if (!CanCraftItem(recipe)) return;

        foreach (var (item, count) in recipe)
        {
            inventory.RemoveItem(item, count);
        }

        inventory.AddAllowedItem(craft);
        inventory.AddItem(craft, 1);
    }

    public bool CanCraftItem(Dictionary<ItemSO, int> recipe)
    {
        foreach (var (item, count) in recipe)
        {
            if (inventory.GetCount(item) <= count) return false;   
        }
        return true;
    }

    
}
