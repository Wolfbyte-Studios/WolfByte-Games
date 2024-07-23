using UnityEngine;
using Unity.Netcode;

public class NetworkObjectIsActive : NetworkBehaviour
{
    public NetworkVariable<bool> isEnabled = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
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
        isEnabled.Value = true;
        this.gameObject.SetActive(true);
    }
    public void OnDisable()
    {
        isEnabled.Value = false;
        this.gameObject.SetActive(false);
    }
}
