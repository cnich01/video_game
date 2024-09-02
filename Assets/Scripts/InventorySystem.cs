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
    public List<ItemSlot> hotbarSlot;
    public List<ItemSlot> backpackSlot;
    public static InventorySystem current;

    public GameObject hotbar;
    public GameObject backpack;
    public Transform itemSelector;
    public PlayerController playerController;

    [SerializeField]
    private int currentSlot;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        inventory = new List<InventoryItem>();
        hotbarSlot = new List<ItemSlot>();
        backpackSlot = new List<ItemSlot>();

        //Create Hotbar Slots
        for (int i = 0; i < hotbar.transform.childCount-1; i++)
        {
            hotbarSlot.Add(hotbar.transform.GetChild(i).GetComponent<ItemSlot>());
        }

        //Create Inventory Slots
        for (int i = 0; i < backpack.transform.childCount; i++)
        {
            if (backpack.transform.GetChild(i).gameObject.activeSelf){
                backpackSlot.Add(backpack.transform.GetChild(i).GetComponent<ItemSlot>());
            }
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
            try
            {
                backpackSlot[GetItemSlot(referenceData)].GetCurrentObject().SetActive(false);
            }
            catch { }
            backpackSlot[GetItemSlot(referenceData)].Set(value, inHandObject);
        }
        else
        {
            InventoryItem newItem = new InventoryItem(referenceData);
            inventory.Add(newItem);
            m_itemDictionary.Add(referenceData, newItem);
            backpackSlot[GetNextSlot()].Set(newItem, inHandObject);
        }
        UpdateHotbar();
    }

    public void Remove(InventoryItemData referenceData)
    {
        if(m_itemDictionary.TryGetValue(referenceData, out InventoryItem value))
        {
            value.RemoveFromStack();

            if (value.stackSize == 0)
            {
                backpackSlot[currentSlot].Reset();
                inventory.Remove(value);
                m_itemDictionary.Remove(referenceData);
            }
            else
            {
                backpackSlot[currentSlot].RemoveLastObject(value);
                playerController.inHandObject = backpackSlot[currentSlot].GetCurrentObject();
            }
            UpdateHotbar();
        }
    }

    public int GetNextSlot()
    {
        for(int i = 0; i< backpackSlot.Count; i++)
        {
            if (backpackSlot[i].GetSprite() == null)
            {
                return i;
            }
        }
        return -1;
    }

    public int GetItemSlot(InventoryItemData itemData)
    {
        for (int i = 0; i < backpackSlot.Count; i++)
        {
            if ((backpackSlot[i].GetSprite() == itemData.icon) && (backpackSlot[i].GetCurrentStackSize() < backpackSlot[i].GetMaxStackSize()))
            {
                return i;
            }
        }
        return GetNextSlot();
    }

    public void UpdateHotbar()
    {
        for (int i = 0; i < hotbarSlot.Count; i++)
        {
            hotbarSlot[i].SetSprite(backpackSlot[i].GetSprite());
            hotbarSlot[i].SetLabel(backpackSlot[i].GetLabel());
            hotbarSlot[i].SetMaxStackSize(backpackSlot[i].GetMaxStackSize());
            hotbarSlot[i].SetObjectList(backpackSlot[i].GetObjectList());
            hotbarSlot[i].SetCurrentStackSize(backpackSlot[i].GetCurrentStackSize());
        }
    }

    public void SetCurrentSlot(int number)
    {
        currentSlot = number;
        for (int i = 0; i < backpackSlot.Count; i++)
        {
            try
            {
                backpackSlot[i].GetCurrentObject().SetActive(false);
            }
            catch  { }
        }
        UpdateHotbar();
        itemSelector.localPosition = new Vector2((130 * currentSlot - 634.24f), 0.25f);
        try
        {
            playerController.inHandObject = hotbarSlot[currentSlot].GetCurrentObject();
            hotbarSlot[currentSlot].GetCurrentObject().SetActive(true);
        }
        catch  { }
    }

    public int GetCurrentSlot()
    {
        return currentSlot;
    }
}
