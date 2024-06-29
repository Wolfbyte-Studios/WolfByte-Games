using UnityEngine;

public class JumpParent : MonoBehaviour
{
    public PlayerMovement pm;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pm = transform.parent.GetComponent<PlayerMovement>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Jump()
    {
        pm.Jump();
    }
    public void StandUp()
    {
        pm.StandUp();
    }
    public void GetUp()
    {
        pm.GetUp();
    }
}
