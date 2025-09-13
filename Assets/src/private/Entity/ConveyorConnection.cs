using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorConnection : MonoBehaviour
{
    [Tooltip("T/F for is receiver")]
    [SerializeField] private bool isReceiver;
    private Inventory inventory;
    private ConveyorBelt belt;

    void Start()
    {
        inventory = GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.Log("Inventory null");
            this.enabled = false;
        }
    }

    void FixedUpdate()
    {
        if (HasItem() && belt.CanTakeMore())
        {
            ItemSO item = GetItem();
            if (!isReceiver)
            {
                belt.GiveItemTo(item, inventory);

            }
            else
            {
                belt.TakeItemFrom(item, inventory);
            }

        }

    }

    private bool HasItem()
    {
        if (inventory.GetTotalCount() > 0)
            return true;

        return false;
    }

    private ItemSO GetItem()
    {
        return inventory.GetFirstItem();
    }

}
