using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    private bool canTakeMore = true;
    private bool canGive = false;

    public List<ItemSO> itemsOnBelt = new();
    public ConveyorBelt nextBelt;
    public LineRenderer lr;

    void Start()
    {
        if (nextBelt != null)
        {
            lr.positionCount = 2;
            lr.SetPosition(0, this.transform.position);
            lr.SetPosition(1, nextBelt.transform.position);

        }

    }

    void FixedUpdate()
    {
        UpdateVars();
        MoveItems();
    }

    public bool CanTakeMore()
    {
        return canTakeMore;
    }

    public void TakeItemFrom(ItemSO item, Inventory inventory)
    {
        Debug.Log("Taking from inv");
        // check if there is extra space (lets do 3 items per grid -> Which currently is each conveyer belt)
        if (canTakeMore)
        {
            Debug.Log("Can take more irtems");
            itemsOnBelt.Add(item);
            inventory.RemoveItem(item);
        }
        else
        {
            Debug.Log("Conveypr belt full");
        }

    }

    public void GiveItemTo(Inventory inventory)
    {
        Debug.Log("giving to inv");
        if (canGive)
        {
            Debug.Log("Giving items");
            ItemSO i = itemsOnBelt[0];
            itemsOnBelt.RemoveAt(0);
            inventory.AddItem(i);
        }

    }

    private void MoveItems()
    {
        foreach (ItemSO item in itemsOnBelt)
        {

            // give to next belt (else the conveyer in will take from the last belt)
            // move shader along

        }
        if (nextBelt == null) return;

        TransferToNextBelt();

    }

    private void TransferToNextBelt()
    {
        if (canGive)
        {
            ItemSO i = itemsOnBelt[0];
            AddItemToNextBelt(i);
        }

    }

    private void AddItemToNextBelt(ItemSO item)
    {
        if (nextBelt.CanTakeMore())
        {
            Debug.Log("Move to next belt");
            nextBelt.AddToBelt(item);
            itemsOnBelt.RemoveAt(0);

        }

    }

    public void AddToBelt(ItemSO item)
    {
        itemsOnBelt.Add(item);
    }


    private void UpdateVars()
    {
        canGive = itemsOnBelt.Count > 0 ? true : false;
        canTakeMore = itemsOnBelt.Count < 3 ? true : false;
    }
}
