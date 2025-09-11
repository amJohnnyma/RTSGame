using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject prefab;
    public int maxStack = 99;

    //Hacky workwaround i dont want to implement a serializable dict
    //crafting recipe
    [Header("Crafting recipe")]
    [Tooltip("List of items required")]
    [SerializeField] private List<ItemSO> item = new();
    [Tooltip("Count of each item (0 or null == 1 item needed)")]
    [SerializeField] private List<int> count = new();
    private Dictionary<ItemSO, int> recipe = new();

    private void OnEnable()
    {
        for (int i = 0; i < item.Count; i++)
        {
            int c = 1;
            if (i < count.Count) c = count[i];

            recipe[item[i]] = c;
        }
    }

    public Dictionary<ItemSO, int> GetCraftingRecipe()
    {
        if (item.Count <= 0) return new();

        return recipe;
    }

}
