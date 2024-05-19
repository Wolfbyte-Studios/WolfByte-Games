using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ButtonGetName : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void OnValidate()
    {
        if (!this.enabled)
        {
            return;
        }
        var txt = gameObject.GetComponent<TextMeshProUGUI>();
        txt.text = GetButtonName();
        txt.enableAutoSizing = true;
    }
    public void OnDrawGizmosSelected()
    {
        OnValidate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public string GetButtonName()
    {
        return gameObject.transform.parent.name;
    }
}