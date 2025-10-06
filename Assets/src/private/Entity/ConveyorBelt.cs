using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BeltItem
{
    public ItemSO item;
    public float progress; // 0 = start, 1 = end
}

public class ConveyorBelt : MonoBehaviour
{
    [Tooltip("Next belts or connections this belt can pass items to.")]
    public List<MonoBehaviour> nextTargets = new(); // can be ConveyorBelt or ConveyorConnection

    [Tooltip("Optional provider connection if this belt pulls directly from an inventory.")]
    public ConveyorConnection provider;

    [Tooltip("Max number of items that can be on this belt at once.")]
    public int capacity = 3;

    [Tooltip("Speed of items along the belt (progress per second).")]
    public float speed = 0.5f;

    public List<BeltItem> itemsOnBelt = new();
    public LineRenderer lr;
    private Vector3 startPos;
    private Vector3 endPos;


    void Start()
    {
        // Store endpoints for fast lookup
        startPos = transform.position;

        if (nextTargets.Count > 0 && nextTargets[0] != null)
            endPos = nextTargets[0].transform.position;
        else
            endPos = startPos + transform.forward * 1f; // fallback

        if (lr != null)
        {
            lr.positionCount = 2; // default line
            lr.SetPosition(0, startPos);
            lr.SetPosition(1, endPos);
        }

        if (provider)
            provider.inUse = true;

    }


// IF HERE NOT DONE -> MAKE PARALLEL SAFE
    public void ConveyerUpdate()
    {
        // pull from provider if available
        if (provider != null)
            TryPullFromConnection(provider);

        MoveItems();
        UpdateLineRenderer();
    }

    private void MoveItems()
    {
        for (int i = 0; i < itemsOnBelt.Count; i++)
        {
            BeltItem beltItem = itemsOnBelt[i];
            beltItem.progress += Time.fixedDeltaTime * speed;

            if (beltItem.progress >= 1f)
            {
                if (TryPassToNext(beltItem.item))
                {
                    itemsOnBelt.RemoveAt(i);
                    i--;
                }
                else
                {
                    // Blocked: hold item at the end until next target can accept
                    beltItem.progress = 0.99f;
                }
            }
        }
    }

    private bool TryPassToNext(ItemSO item)
    {
        foreach (var target in nextTargets)
        {
            if (target == null) continue;

            if (target is ConveyorBelt nextBelt && nextBelt.CanTakeMore())
            {
                nextBelt.AddToBelt(item);
                return true;
            }
            else if (target is ConveyorConnection conn && conn.IsReceiver && conn.TryReceive(item))
            {
                return true;
            }
        }
        return false;
    }

    public bool CanTakeMore() => itemsOnBelt.Count < capacity;

    public void AddToBelt(ItemSO item)
    {
        if (CanTakeMore())
            itemsOnBelt.Add(new BeltItem { item = item, progress = 0f });
    }

    private void TryPullFromConnection(ConveyorConnection connection)
    {
        if (!CanTakeMore() || connection == null || connection.IsReceiver) return;

        ItemSO item = connection.TryProvide();
        if (item != null)
            AddToBelt(item);
    }

        /// Updates the LineRenderer to show each item along the belt path.
    private void UpdateLineRenderer()
    {
        if (lr == null) return;

        if (itemsOnBelt.Count == 0)
        {
            // fallback: just show base belt line
            lr.positionCount = 2;
            lr.SetPosition(0, startPos);
            lr.SetPosition(1, endPos);
            return;
        }

        lr.positionCount = itemsOnBelt.Count;
        for (int i = 0; i < itemsOnBelt.Count; i++)
        {
            Vector3 pos = Vector3.Lerp(startPos, endPos, itemsOnBelt[i].progress);
            lr.SetPosition(i, pos);
        }
    }
}
