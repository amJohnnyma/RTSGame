using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public class ItemStack
{
    public ItemSO item;
    public int count;
}

[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    private Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
    public Dictionary<TKey, TValue> Dictionary => dict;

    public void OnBeforeSerialize()
    {
        // Push runtime dict into serialized lists
        keys.Clear();
        values.Clear();
        foreach (var kvp in dict)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        // Rebuild dict from serialized lists
        dict = new Dictionary<TKey, TValue>();
        for (int i = 0; i < Math.Min(keys.Count, values.Count); i++)
        {
            if (!dict.ContainsKey(keys[i]))
                dict.Add(keys[i], values[i]);
        }
    }

    //Helpers to keep both in sync
    public void Set(TKey key, TValue value)
    {
        dict[key] = value;
        SyncLists();
    }

    public bool Remove(TKey key)
    {
        if (dict.Remove(key))
        {
            SyncLists();
            return true;
        }
        return false;
    }

    private void SyncLists()
    {
        keys.Clear();
        values.Clear();
        foreach (var kvp in dict)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }
}


public abstract class Inventory : MonoBehaviour
{
    [Header("Storage Settings")]
    [Tooltip("Items that this inventory accepts. Leave empty to allow all.")]
    public List<ItemSO> allowedItems = new List<ItemSO>();

    [SerializeField] private SerializableDictionary<ItemSO, int> itemStorage = new SerializableDictionary<ItemSO, int>();

    private Dictionary<ItemSO, int> Items => itemStorage.Dictionary;

    public void AddItem(ItemSO item, int count = 1)
    {
        if (allowedItems.Count > 0 && !allowedItems.Contains(item))
        {
            Debug.LogWarning($"{item.name} is not allowed in {gameObject.name}'s inventory.");
            return;
        }

        int existing = GetCount(item);
        int newCount = existing + count;

        if (newCount <= 0)
            itemStorage.Remove(item);
        else
            itemStorage.Set(item, newCount);

        OnInventoryChanged();
    }

    public bool RemoveItem(ItemSO item, int count = 1)
    {
        int existing = GetCount(item);
        if (existing < count)
            return false;

        int newCount = existing - count;

        if (newCount <= 0)
            itemStorage.Remove(item);
        else
            itemStorage.Set(item, newCount);

        OnInventoryChanged();
        return true;
    }

    public int GetCount(ItemSO item) =>
        Items.TryGetValue(item, out int count) ? count : 0;

    public int GetCount(string name)
    {
        return GetCount(GetItemSO(name));
    }

    public bool IsEmpty(string name)
    {
        return GetCount(name) <= 0;
    }

    public IReadOnlyDictionary<ItemSO, int> GetAllItems() => Items;

    public void GiveItemToOther(string itemName, int amount, Inventory other)
    {
        ItemSO item = GetItemSO(itemName);
        if (other.allowedItems.Count > 0 && !other.allowedItems.Contains(item))
            return;


        int myCount = GetCount(item);
        int theirCount = other.GetCount(item);

        int canGive = Mathf.Min(myCount, amount);
        int canReceive = item.maxStack - theirCount;
        int transferAmount = Mathf.Min(canGive, canReceive);

        if (transferAmount <= 0) return;

        RemoveItem(item, transferAmount);
        other.AddItem(item, transferAmount);
    }

    public ItemSO GetItemSO(string name)
    {
        for (int i = 0; i < allowedItems.Count; i++)
        {
            if (allowedItems[i].itemName == name)
                return allowedItems[i];
        }
        return null;
    }

    public abstract void OnInventoryChanged();
}


[CustomPropertyDrawer(typeof(SerializableDictionary<ItemSO, int>))]
public class ItemDictionaryDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty keys = property.FindPropertyRelative("keys");
        int rows = keys.arraySize;
        return EditorGUIUtility.singleLineHeight * (rows + 2) + 6;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty keys = property.FindPropertyRelative("keys");
        SerializedProperty values = property.FindPropertyRelative("values");

        EditorGUI.LabelField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), label);
        position.y += EditorGUIUtility.singleLineHeight + 2;

        int count = Mathf.Min(keys.arraySize, values.arraySize);
        float half = (position.width - 20) / 2f;

        // Try to fetch allowedItems from parent object
        Inventory inv = property.serializedObject.targetObject as Inventory;
        var allowed = inv != null ? inv.allowedItems : null;

        for (int i = 0; i < count; i++)
        {
            Rect row = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // Item dropdown
            SerializedProperty keyProp = keys.GetArrayElementAtIndex(i);
            ItemSO currentItem = keyProp.objectReferenceValue as ItemSO;

            if (allowed != null && allowed.Count > 0)
            {
                // Build popup options
                string[] options = new string[allowed.Count];
                int currentIndex = -1;
                for (int j = 0; j < allowed.Count; j++)
                {
                    options[j] = allowed[j] != null ? allowed[j].name : "None";
                    if (allowed[j] == currentItem) currentIndex = j;
                }

                int newIndex = EditorGUI.Popup(
                    new Rect(row.x, row.y, half, row.height),
                    currentIndex, options
                );

                if (newIndex >= 0 && newIndex < allowed.Count)
                    keyProp.objectReferenceValue = allowed[newIndex];
            }
            else
            {
                // fallback: free ObjectField
                EditorGUI.PropertyField(new Rect(row.x, row.y, half, row.height),
                    keyProp, GUIContent.none);
            }

            // Count field
            EditorGUI.PropertyField(new Rect(row.x + half + 5, row.y, half, row.height),
                values.GetArrayElementAtIndex(i), GUIContent.none);

            SerializedProperty valueProp = values.GetArrayElementAtIndex(i);
            int currentCount = valueProp.intValue;

            Rect countRect = new Rect(row.x + half + 5, row.y, half - 20, row.height);
            int newCount = EditorGUI.IntField(countRect, currentCount);

            // Clamp to stack size if item is valid
            if (currentItem != null)
            {
                int maxStack = Mathf.Max(1, currentItem.maxStack); // safeguard
                newCount = Mathf.Clamp(newCount, 0, maxStack);
            }
            else
            {
                newCount = Mathf.Max(0, newCount);
            }

            valueProp.intValue = newCount;

            // Remove button
            if (GUI.Button(new Rect(row.x + row.width - 18, row.y, 18, row.height), "x"))
            {
                keys.DeleteArrayElementAtIndex(i);
                values.DeleteArrayElementAtIndex(i);
                break;
            }

            position.y += EditorGUIUtility.singleLineHeight + 2;
        }

        // Add button
        if (GUI.Button(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), "+"))
        {
            keys.arraySize++;
            values.arraySize++;

            var newKey = keys.GetArrayElementAtIndex(keys.arraySize - 1);
            var newValue = values.GetArrayElementAtIndex(values.arraySize - 1);

            if (allowed != null && allowed.Count > 0)
                newKey.objectReferenceValue = allowed[0];  // pick first allowed item
            else
                newKey.objectReferenceValue = null;        // fallback

            newValue.intValue = 0;                         // start with zero count

            property.serializedObject.ApplyModifiedProperties();
        }
    }
}