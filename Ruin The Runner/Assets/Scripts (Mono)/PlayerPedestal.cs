using System.Collections.Generic;
using UnityEngine;

public class PlayerPedestal : MonoBehaviour
{
    [Header("Which players are accepted:")]
    public bool p1;
    public bool p2;     
    public bool p3;
    public bool p4;
    public List<int> AcceptedPlayers = new List<int>();
    public List<PlayerMovement.playertype> AcceptedPlayertypes = new List<PlayerMovement.playertype>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AcceptedPlayers.Clear();
        if (p1) { AcceptedPlayers.Add(0); }
        if (p2) { AcceptedPlayers.Add(1); }
        if (p3) { AcceptedPlayers.Add(2); }
        if (p4) { AcceptedPlayers.Add(3); }
    }

    // Update is called once per frame
    void Update()
    {
        
 
    }
}
