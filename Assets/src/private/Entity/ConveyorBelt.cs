using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    private bool canTakeMore = false;
    private bool canGive = false;

    private List<ItemSO> itemsOnBelt = new();
    private ConveyorBelt nextBelt;

    void FixedUpdate()
    {
        MoveItems();
    }

    public bool CanTakeMore()
    {
        return canTakeMore;
    }

    public void TakeItemFrom(ItemSO item, Inventory inventory)
    {
        // check if there is extra space (lets do 3 items per grid -> Which currently is each conveyer belt)

    }

    public void GiveItemTo(ItemSO item, Inventory inventory)
    {

    }

    private void MoveItems()
    {
        foreach (ItemSO item in itemsOnBelt)
        {
            // move shader along
        }

    }
}
