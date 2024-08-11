using Mirror;
 
using UnityEngine;


public class Poop : NetworkBehaviour
{
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnStartClient()
    {
        base.OnStartClient();
        
    }
    public void Start()
    {
        GameManager.singleton.poopList.Add(this);
        GameManager.singleton.lastPooped = GameManager.singleton.poopList[GameManager.singleton.poopList.Count - 1].transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
