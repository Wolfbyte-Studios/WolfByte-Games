using UnityEngine;

public class LookAtRunner : MonoBehaviour
{
    public GameObject target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var runners = GameObject.FindGameObjectsWithTag("Runner");
        float shortestDistance = Mathf.Infinity;
        GameObject closestRunner = null;

        foreach (var runner in runners)
        {
            float distanceToRunner = Vector3.Distance(transform.position, runner.transform.position);
            if (distanceToRunner < shortestDistance)
            {
                shortestDistance = distanceToRunner;
                closestRunner = runner;
            }
        }

        if (closestRunner != null)
        {
            target = closestRunner;
            transform.LookAt(target.transform.position + new Vector3(0, 2, 0));
        }
    }
}
