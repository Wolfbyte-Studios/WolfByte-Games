using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.Netcode.Components;
using System.Collections;
using Unity.VisualScripting;

public class Dizzy : NetworkBehaviour
{
    public float time = 1f;
    public Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        anim = GetComponent<PlayerMovement>().anim;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void TriggerDizzy()
    {
        StartCoroutine(dizziness());
    }

    IEnumerator dizziness()
    {
        //add dizzy
        anim.SetBool("Dizzy", true);
        gameObject.GetComponent<PlayerMovement>().Dizzy = true;
        yield return new WaitForSeconds(time);
        //end dizzy
        anim.SetBool("Dizzy", false);

        yield return null;
    }
}
