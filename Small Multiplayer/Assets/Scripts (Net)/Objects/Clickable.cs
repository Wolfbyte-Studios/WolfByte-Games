using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Clickable : NetworkBehaviour
{
    public UnityEvent myEvent;
    public bool CoolDown;
    public float coolDown;
    public float timeFired;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void TriggerEvent()
    {
        if (myEvent != null)
        {
            if (CoolDown)
            {
                if (coolDown > Time.time - timeFired)
                {
                    return;
                }
            }
            timeFired = Time.time;
            Debug.Log("Activation should happen");
            myEvent.Invoke();

        }
    }
}
