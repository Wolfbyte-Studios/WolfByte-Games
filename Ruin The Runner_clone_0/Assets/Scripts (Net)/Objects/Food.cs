using Mirror;
using UnityEngine;

public class Food : NetworkBehaviour
{
    public int FoodPoints = 10;
    public bool doesRespawn;
    public int RespawnTime = 60;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Runner")
        {

            NetworkUtils.RpcHandler(this, EatFood);

        }
        else
        {
            Debug.LogWarning("Food collided with non-runner object: " + collision.name + " " + collision.tag);
        }
    }
    public void EatFood()
    {
        GameManager.singleton.AddHunger(FoodPoints);
    }
}
