using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Fizzle : NetworkBehaviour
{
    public float countDown;
    public float collisionTime;
    public bool collided;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (collided)
        {
            gameObject.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, Vector3.zero, .01f);
        }
    }
    public IEnumerator fizzle()
    {
        yield return new WaitForSeconds(countDown);
            
        gameObject.GetComponent<NetworkObject>().Despawn();
        //Destroy(gameObject);
        yield return null;
    }
    public void OnCollisionEnter(Collision collision)
    {
        collided = true;
        collisionTime = Time.time;
        StartCoroutine(fizzle());
    }
}
