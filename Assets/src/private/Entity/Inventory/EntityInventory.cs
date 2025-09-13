using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityInventory : Inventory
{

    override public void OnInventoryChanged()
    {
        totalItemCount = 0;
        foreach (var (k, v) in Items)
        {
            totalItemCount += v;
        }

    }

}
