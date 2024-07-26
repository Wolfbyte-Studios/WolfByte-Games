using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Clickable : NetworkBehaviour
{
    public static int ClicksLeft;
    public bool ClicksCounted;
    public UnityEvent myEvent;
    public UnityEvent OnCoolDown;
    public UnityEvent onSelect;
    public UnityEvent onDeselect;
    public UnityEvent secondaryEvent; // Add secondary event
    public bool needsToCoolDown;
    public bool CoolDown;
    public float coolDown;
    [Range(0f, 1f)]
    public float coolDownEarlyPercentage = 1;
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

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        OldMat = meshRenderer.sharedMaterial;
        NewMat = new Material(OldMat.shader);
        timeFired = -100000;
    }

    void Update()
    {
        timeElapsed = Time.time - timeFired;
        percentageFinished = timeElapsed / coolDown;
        if (percentageFinished >= 1)
        {
            if (needsToCoolDown)
            {
                coolingDown();
            }
            meshRenderer.material = OldMat;
            return;
        }
        else
        {
            if (percentageFinished >= coolDownEarlyPercentage)
            {
                OnCoolDown.Invoke();
            }
            meshRenderer.material = NewMat;
            applyColors(percentageFinished);
        }
    }

    public void coolingDown()
    {
        OnCoolDown.Invoke();
        needsToCoolDown = false;
    }

    public void applyColors(float percent)
    {
        percent = percent * 100f;
        switch (percent)
        {
            case 0:
                NewMat.color = low;
                return;
            case <= 40:
                NewMat.color = low;
                return;
            case <= 70:
                NewMat.color = med;
                return;
            case <= 95:
                NewMat.color = high;
                return;
            case > 95:
                NewMat.color = done;
                return;
        }
    }

    public void TriggerEvent()
    {
        if (myEvent != null)
        {
            if (ClicksLeft == 0 && ClicksCounted)
            {
                return;
            }
            if (CoolDown)
            {
                if (coolDown >= Time.time - timeFired)
                {
                    return;
                }
            }
            timeFired = Time.time;
            ////Debug.Log("Activation should happen");
            myEvent.Invoke();
            if (ClicksCounted)
            {
                ClicksLeft--;
            }
            needsToCoolDown = true;
        }
    }

    public void TriggerSecondary()
    {
        if (secondaryEvent != null)
        {
            secondaryEvent.Invoke();
        }
    }

    public void OnSelect()
    {
        onSelect.Invoke();
    }

    public void OnDeselect()
    {
        onDeselect.Invoke();
    }
}
