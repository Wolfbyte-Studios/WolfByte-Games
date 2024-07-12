using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Clickable : NetworkBehaviour
{
    public UnityEvent myEvent;
    public bool CoolDown;
    public float coolDown;
    public float timeFired;
    public float timeElapsed;
    public float percentageFinished;

    public Material NewMat;
    public Material OldMat;
    public MeshRenderer meshRenderer;
    public Color low;
    public Color med;
    public Color high;
    public Color done;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        meshRenderer = GetComponent<MeshRenderer>();
        OldMat = meshRenderer.sharedMaterial;
        NewMat = new Material(OldMat.shader);
        timeFired = 1;
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed = Time.time - timeFired;
        percentageFinished = timeElapsed / coolDown;
        if (percentageFinished >= 1)
        {
            meshRenderer.material = OldMat;
            return;
        }
        else
        {
            meshRenderer.material = NewMat;
            applyColors(percentageFinished);
        }
    }
    public void applyColors(float percent)
    {
        percent = percent * 100f;
        switch (percent)
        {
            case  0:
                NewMat.color = low;
                return;
            case <= 40:
                NewMat.color = low;
                return;
            case <= 70:
                NewMat.color = med;
                return;
            case <= 90:
                NewMat.color = high;
                return;
            case > 90:
                NewMat.color = done;
                return;
        }
    }
    public void TriggerEvent()
    {
        if (myEvent != null)
        {
            if (CoolDown)
            {
                if (coolDown >= Time.time - timeFired)
                {
                    return;
                }
            }
            timeFired = Time.time;
            Debug.Log("Activation should happen");
            myEvent.Invoke();

        }
    }
}
