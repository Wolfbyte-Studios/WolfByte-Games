using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public GameObject playerCam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCam = transform.parent.parent.FindDeepChildByTag("MainCamera").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.eulerAngles = new Vector3(playerCam.transform.eulerAngles.x, playerCam.transform.eulerAngles.y, playerCam.transform.eulerAngles.z);
    }
}
