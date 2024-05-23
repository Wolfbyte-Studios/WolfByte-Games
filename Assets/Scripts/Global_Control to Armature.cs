using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global_ControltoArmature : MonoBehaviour
{
    public Transform gc;
    // Start is called before the first frame update
    void Awake()
    {
        gc = transform.Find("Global_Control");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        gameObject.transform.SetPositionAndRotation(gc.position, gc.localRotation);
    }
}
