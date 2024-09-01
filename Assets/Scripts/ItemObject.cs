using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObject : MonoBehaviour
{
    public InventoryItemData referenceItem;

    public void OnHandlePickupItem(GameObject inHandObject)
    {
        InventorySystem.current.Add(referenceItem, inHandObject);
    }
    public void OnHandleDropItem()
    {
        InventorySystem.current.Remove(referenceItem);
    }
}
