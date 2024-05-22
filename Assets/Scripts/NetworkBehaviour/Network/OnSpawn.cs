using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{

    public GameObject Player1;
    public GameObject Player2;
    public bool swapPlayers;
    public enum Characters {
        Raz,
        Zara
    }
    public Characters CurrentCharacter;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        Player1.SetActive(false);
        Player2.SetActive(false);
        var playerCam = GameObject.Find("PlayerCam");
        if (OwnerClientId == 0)
        {
            Debug.Log("Player1");
            gameObject.name = "Wrapper1";
            Debug.Log(gameObject.name + " Is host!");
            CurrentCharacter = Characters.Raz;
        }
        if (OwnerClientId == 1)
        {
            Debug.Log("Player2");
            gameObject.name = "Wrapper2";
            Debug.Log(gameObject.name + " Is client!");
            CurrentCharacter = Characters.Zara;

        }
       
        if (OwnerClientId == 0)
        {
            Debug.Log(playerCam.transform.position);
            Player1.SetActive(true);
            Player1.transform.FindDeepChild("Raz").GetComponent<PlayerMovement>().playerSpawnStuff(playerCam);
        }
       if(OwnerClientId == 1)
        {
            Player2.SetActive(true);
            Player2.transform.FindDeepChild("Zara").GetComponent<PlayerMovement>().playerSpawnStuff(playerCam);
        }

    }

    // Update is called once per frame
    void Update()
    {
       
        if (swapPlayers) 
        {
            switch (CurrentCharacter)
            {
                case Characters.Raz:
                    Player1.SetActive(true);
                    Player2.SetActive(false);
                    break;
                case Characters.Zara:
                    Player1.SetActive(false);
                    Player2.SetActive(true);
                    break;
            }
    }
    }
    
}
