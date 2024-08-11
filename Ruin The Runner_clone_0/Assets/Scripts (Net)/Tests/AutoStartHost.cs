using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AutoStartHost : MonoBehaviour
{
    public UnityEvent onStart;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!SteamManager.Initialized) { return; }
        StartCoroutine(delay());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public IEnumerator delay()
    {
        yield return new WaitForSeconds(2);
        onStart.Invoke();
        yield return null;
    }
}
