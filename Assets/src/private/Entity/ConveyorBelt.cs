using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    private bool canTakeMore = true;
    private bool canGive = false;
    private bool isTransferring = false;

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
            AddToBelt(item);
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
        if (canGive && itemsOnBelt.Count > 0)
        {
            Debug.Log("Giving items");
            ItemSO i = itemsOnBelt[0];
            if (inventory.GetCount(i) >= i.maxStack) return;
            RemoveFromBelt();
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
            RemoveFromBelt();

        }

    }

    public void AddToBelt(ItemSO item)
    {
        if(!isTransferring)
            StartCoroutine(AddToBeltWithDelay(item));
    }

    private IEnumerator AddToBeltWithDelay(ItemSO item)
    {
        isTransferring = true;
        yield return new WaitForSeconds(0.2f); // 200ms
        if (itemsOnBelt.Count < 3)
            itemsOnBelt.Add(item);

        isTransferring = false;
    }

    public void RemoveFromBelt()
    {
        StartCoroutine(RemoveFromBeltWithDelay());
    }

    private IEnumerator RemoveFromBeltWithDelay()
    {
        isTransferring = true;
        yield return new WaitForSeconds(0.2f); // 200ms
        if (itemsOnBelt.Count > 0)
            itemsOnBelt.RemoveAt(0);
        isTransferring = false;
    }



    private void UpdateVars()
    {
        canGive = itemsOnBelt.Count > 0;
        canTakeMore = itemsOnBelt.Count < 3;
    }
}
