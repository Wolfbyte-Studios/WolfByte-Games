using UnityEngine;
using Mirror;

public class NetworkObjectIsActive : NetworkBehaviour
{
    [SyncVar]
    public bool isEnabled = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnEnable()
    {
        isEnabled  = true;
        this.gameObject.SetActive(true);
    }
    public void OnDisable()
    {
        isEnabled  = false;
        this.gameObject.SetActive(false);
    }
}
