using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetIPandPort : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GetNameSetVariable()
    {
        var name = gameObject.name;
        if (name.Contains("IP"))
        {
            LoadTheLoadingScreen.IP = gameObject.GetComponent<TMP_InputField>().text;
        }
        if (name.Contains("Port"))
        {
            LoadTheLoadingScreen.Port =int.Parse(gameObject.GetComponent<TMP_InputField>().text);
        }

    }
}
