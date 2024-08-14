using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    public List<PlayerMovement.playertype> acceptedPlayers = new List<PlayerMovement.playertype>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.GetAllComponentsInHierarchy<PlayerMovement>().Count < 0 && !other.name.ToLower().Contains("toucher"))
        {
            var pM = other.gameObject.transform.GetAllComponentsInHierarchy<PlayerMovement>()[0];
            if (acceptedPlayers.Contains(pM.PlayerType))
            {
                pM.Respawn();
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
