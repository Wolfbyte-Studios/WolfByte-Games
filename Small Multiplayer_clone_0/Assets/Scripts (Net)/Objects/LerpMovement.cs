using System.Collections.Generic;
using UnityEngine;
using Mirror;
 


public class LerpMovement : NetworkBehaviour
{
    [System.Serializable]
    public struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public float speed;
    }
    
    public List<TransformData> transforms = new List<TransformData>();
    private TransformData initialTransform;
    private bool isLerping = false;
    public float lerpSpeed = 1.0f;
    public bool Stationary = true;
    private int currentTargetIndex = 0;
    public bool rigBody = true;
    private Rigidbody rb;
    

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (rigBody)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = Stationary;
        }
        initialTransform.position = transform.position;
        initialTransform.rotation = transform.rotation;
        transform.position = initialTransform.position;
      
    }

    

    [ClientRpc]
    private void ApplyInitialTransformClientRpc(Vector3 position, Quaternion rotation, ulong clientId)
    {
        if (this.gameObject.GetComponent<NetworkIdentity>().netId == clientId)
        {
            transform.position = position;
            transform.rotation = rotation;
            initialTransform.position = position;
            initialTransform.rotation = rotation;
        }
    }

    void Update()
    {
        if (isServer && isLerping)
        {
            
            if (rigBody)
            {
                LerpToTarget();
                rb.isKinematic = Stationary;
            }
            else
            {
                LerpToTargetNoRB();
            }
    }
    }

    
    public void TriggerServerRpc()
    {
        if (transforms.Count > 0)
        {
            isLerping = true;
            currentTargetIndex = 0;
            TriggerClientRpc();
        }
    }

    [ClientRpc]
    private void TriggerClientRpc()
    {
        if (!isServer)
        {
            isLerping = true;
            currentTargetIndex = 0;
        }
    }

    public void Trigger()
    {
       
            isLerping = true;
            currentTargetIndex = 0;
            TriggerClientRpc();
        
    }
    public void LerpToTargetNoRB()
    {
        TransformData targetTransform = currentTargetIndex < transforms.Count ? transforms[currentTargetIndex] : initialTransform;
        if (targetTransform.speed == 0)
        {
            targetTransform.speed = 1;
        }
        transform.position = Vector3.Lerp(transform.position, targetTransform.position, Time.deltaTime * lerpSpeed * targetTransform.speed);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetTransform.rotation, Time.deltaTime * lerpSpeed * targetTransform.speed);

        if (Vector3.Distance(transform.position, targetTransform.position) < 0.1f && Quaternion.Angle(transform.rotation, targetTransform.rotation) < 1.0f)
        {
            currentTargetIndex++;
            if (currentTargetIndex > transforms.Count)
            {
                isLerping = false;
                currentTargetIndex = 0;
            }
        }
    }
    public void LerpToTarget()
    {
        TransformData targetTransform = currentTargetIndex < transforms.Count ? transforms[currentTargetIndex] : initialTransform;
        if (targetTransform.speed == 0)
        {
            targetTransform.speed = 1;
        }

        Vector3 newPosition = Vector3.Lerp(transform.position, targetTransform.position, Time.deltaTime * lerpSpeed * targetTransform.speed);
        Quaternion newRotation = Quaternion.Lerp(transform.rotation, targetTransform.rotation, Time.deltaTime * lerpSpeed * targetTransform.speed);

        rb.MovePosition(newPosition);
        rb.MoveRotation(newRotation);

        if (Vector3.Distance(transform.position, targetTransform.position) < 0.1f && Quaternion.Angle(transform.rotation, targetTransform.rotation) < 1.0f)
        {
            currentTargetIndex++;
            if (currentTargetIndex > transforms.Count)
            {
                isLerping = false;
                currentTargetIndex = 0;
            }
        }
    }

    

   
}
