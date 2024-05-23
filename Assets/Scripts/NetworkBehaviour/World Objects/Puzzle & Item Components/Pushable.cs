using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Pushable : NetworkBehaviour
{
    public Rigidbody rb;
    public float pushScale;
    public GameObject pusher;
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerStay(Collider other)
    {
        switch(other.tag )

        {
            case "Player":
                Pushing(other.gameObject, true);
                break;
            case "Player1":
                Pushing(other.gameObject, true);
                break;
            case "Player2":
                Pushing(other.gameObject, true);
                break;

            case "Pusher":
                rb.linearVelocity = CalculateVelocity();
                break;
        }
    }
    public void Pushing(GameObject Player, bool isActive)
    {
        Player.GetComponent<PlayerMovement>().animator.SetBool("Push", isActive);
        Player.GetComponent<PlayerMovement>().animator.SetFloat("Speed", Mathf.Clamp(Player.GetComponent<PlayerMovement>().animator.GetFloat("Speed"), 0, 1));
        player = Player.gameObject;
        foreach (Transform pusher in Player.transform.FindDeepChildrenByTag("Pusher"))
        {
            pusher.gameObject.SetActive(isActive);
        }
    }
    public void OnTriggerExit(Collider other)
    {
        switch (other.tag)

        {
            case "Player":
                Pushing(other.gameObject, false);
                break;
            case "Player1":
                Pushing(other.gameObject, false);
                break;
            case "Player2":
                Pushing(other.gameObject, false);
                break;
        }
    }
    public Vector3 CalculateVelocity()
    {
        Vector3 velocity = (player.GetComponent<PlayerMovement>().MoveDirection * pushScale * (1 / gameObject.GetComponent<MaterialType>().baseMass));
        velocity = new Vector3(Mathf.Round(velocity.x), Mathf.Round(velocity.y), Mathf.Round(velocity.z));
        return velocity;
    }
    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.tag == "Pusher")
        {
            Debug.Log("Pushed! " + CalculateVelocity());
            rb.linearVelocity = CalculateVelocity();


        }

    }
}
