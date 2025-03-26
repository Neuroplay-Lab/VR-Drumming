using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusItem : MonoBehaviour
{
    [SerializeField] private string itemLabel;

    public void Reset()
    {
        itemLabel = this.transform.name;
    }

    public string GetItemLabel()
    {
        return itemLabel;
    }
}
