using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ConveyorConnection : MonoBehaviour
{
    [Tooltip("T/F for is receiver")]
    [SerializeField] private bool isReceiver;
    [SerializeField] private ItemSO craftedOut;
    private Inventory inventory;
    // public temp for now
    public ConveyorBelt belt;
    // temporary connection drawing
    public LineRenderer lr;

    void Start()
    {
        inventory = GetComponentInParent<Inventory>();
        if (inventory == null)
        {
            Debug.Log("Inventory null");
            this.enabled = false;
            return;
        }
        if (belt == null) return;
        lr.positionCount = 2;
        lr.SetPosition(0, this.transform.position);
        lr.SetPosition(1, belt.transform.position);

        if (belt != null)
            StartCoroutine(RunConveyor()); // start once

    }

    private IEnumerator RunConveyor()
    {
        while (true) // keep running
        {
            if (belt == null)
            {
                Debug.Log("Null belt");
                yield break; // stop forever
            }

            if (isReceiver)
            {
                Debug.Log("Receiver");
                belt.GiveItemTo(inventory);
            }
            else
            {
                if (HasItem() && belt.CanTakeMore())
                {
                    ItemSO item = GetItem();
                    if (item != null)
                        belt.TakeItemFrom(item, inventory);
                }
            }

            // wait 200ms before trying again
            yield return new WaitForSeconds(0.2f);
        }
    }
    // ** NBNBNBNBNBNNBNBNBNBNB
    // ConveyerConnection must be on a child such that there can be a input and output 
    // this can be made into a red/blue cube prefab for now


    private bool HasItem()
    {
        if (inventory.GetTotalCount() > 0)
            return true;

        return false;
    }

    private ItemSO GetItem()
    {
        if (craftedOut == null)
        {
            return inventory.GetFirstItem();
        }
        if (inventory.IsEmpty(craftedOut)) return null;
        return craftedOut;
    }

}
