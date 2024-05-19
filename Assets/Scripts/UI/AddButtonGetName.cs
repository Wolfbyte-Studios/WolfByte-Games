using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
public class AddButtonGetName : MonoBehaviour
{
    public List<GameObject> texts = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.FindDeepChildrenByType<TextMeshProUGUI>();
    }
    public void OnValidate()
    {
        foreach(Transform t in transform.FindDeepChildrenByType<TextMeshProUGUI>())
        {
            if (texts.Contains(t.gameObject))
            {
                return;
            }
            texts.Add(t.gameObject);
            if(t.GetComponent<ButtonGetName>() == null)
            {
                t.AddComponent<ButtonGetName>();
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
