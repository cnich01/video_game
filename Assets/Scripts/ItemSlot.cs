using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class ItemSlot : MonoBehaviour
{

    [SerializeField]
    private Image m_icon;

    [SerializeField]
    private Text m_label;

    [SerializeField]
    private List<GameObject> m_stackObj;

    [SerializeField]
    private Text m_stackLabel;


    public void Awake()
    {
        Reset();
    }
    public void Set(InventoryItem item, GameObject inHandObject)
    {
        m_icon.sprite = item.data.icon;
        m_icon.enabled = true;
        m_label.text = item.data.displayName;
        m_stackObj.Add(inHandObject);
        if (item.stackSize > 1)
        {
            m_stackLabel.text = item.stackSize.ToString();
        }
        else if (item.stackSize == 1)
        {
            m_stackLabel.text = null;
        }
        else
        {
            Reset();
        }
        
    }

    public void Reset()
    {
        m_icon.sprite = null;
        m_icon.enabled = false;
        m_label.text = null;
        m_stackObj = new List<GameObject>();
        m_stackLabel.text = null;
    }

    public void RemoveLastObject(InventoryItem item)
    {
        m_stackObj.Remove(GetObject());
        if (item.stackSize > 1)
        {
            m_stackLabel.text = item.stackSize.ToString();
        }
        else if (item.stackSize == 1)
        {
            m_stackLabel.text = null;
        }
        else
        {
            Reset();
        }
    }

    public GameObject GetObject()
    {
        try
        {
            return m_stackObj[m_stackObj.Count - 1];
        }
        catch
        {
            return null;
        }
    }

    public Sprite GetSprite()
    {
        return m_icon.sprite;
    }
}
