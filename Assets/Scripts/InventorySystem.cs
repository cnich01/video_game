using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using System;
using UnityEditorInternal.Profiling.Memory.Experimental;

public class InventorySystem : MonoBehaviour
{
    private Dictionary<InventoryItemData, InventoryItem> m_itemDictionary;
    public List<InventoryItem> inventory { get; private set; }
    public List<ItemSlot> slot;
    public static InventorySystem current;

    public GameObject hotbar;
    public Transform itemSelector;
    public PlayerMovement playerController;

    [SerializeField]
    private int currentSlot;

    private void Awake()
    {
        playerController = GetComponent<PlayerMovement>();
        inventory = new List<InventoryItem>();
        slot = new List<ItemSlot>();

        for (int i = 0; i < hotbar.transform.childCount-1; i++)
        {
            slot.Add(hotbar.transform.GetChild(i).GetComponent<ItemSlot>());
        }

        m_itemDictionary = new Dictionary<InventoryItemData, InventoryItem>();

        current = this;
    }

    public InventoryItem Get(InventoryItemData referenceData)
    {
        if(m_itemDictionary.TryGetValue(referenceData, out InventoryItem value))
        {
            return value;
        }
        return null;
    }
    
    public void Add(InventoryItemData referenceData, GameObject inHandObject)
    {
        if (m_itemDictionary.TryGetValue(referenceData, out InventoryItem value))
        {
            value.AddToStack();
            slot[GetItemSlot(referenceData)].Set(value, inHandObject);
        }
        else
        {
            InventoryItem newItem = new InventoryItem(referenceData);
            inventory.Add(newItem);
            m_itemDictionary.Add(referenceData, newItem);
            slot[GetNextSlot()].Set(newItem, inHandObject);
        }
    }

    public void Remove(InventoryItemData referenceData)
    {
        if(m_itemDictionary.TryGetValue(referenceData, out InventoryItem value))
        {
            value.RemoveFromStack();

            if (value.stackSize == 0)
            {
                slot[currentSlot].Reset();
                inventory.Remove(value);
                m_itemDictionary.Remove(referenceData);
            }
            else
            {
                slot[currentSlot].RemoveLastObject(value);
                playerController.inHandObject = slot[currentSlot].GetObject();
            }
        }
    }

    public int GetNextSlot()
    {
        for(int i = 0; i<slot.Count; i++)
        {
            if (slot[i].GetSprite() == null)
            {
                return i;
            }
        }
        return -1;
    }

    public int GetItemSlot(InventoryItemData itemData)
    {
        for (int i = 0; i < slot.Count; i++)
        {
            if ((slot[i].GetSprite() == itemData.icon) && (slot[i].GetStackSize() < slot[i].GetMaxStack()))
            {
                return i;
            }
        }
        return GetNextSlot();
    }

    public void SetCurrentSlot(int number)
    {
        currentSlot = number;
        for (int i = 0; i < slot.Count; i++)
        {
            try
            {
                slot[i].GetObject().SetActive(false);
            }
            catch  { }
        }
        itemSelector.localPosition = new Vector2((130 * currentSlot - 634.24f), 0.25f);
        try
        {
            playerController.inHandObject = slot[currentSlot].GetObject();
            slot[currentSlot].GetObject().SetActive(true);
        }
        catch  { }
    }

    public int GetCurrentSlot()
    {
        return currentSlot;
    }
}
