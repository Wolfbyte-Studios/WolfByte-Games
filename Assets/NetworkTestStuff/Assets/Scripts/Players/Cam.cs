using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public Transform playerTransform;
    public float smoothing = 5.0f;
    public float damping = 2.0f;
    public float rotationSpeed = 5.0f;
    public float zoomSpeed = 5.0f;
    public Vector3 offset;
    public Vector2 rotation = Vector2.zero;
    public float camRotateSensitivity;
    public float zoomDistanceScale = 9f;
    public Vector2 CameraRotMax = new Vector2(15, 70);
    public Vector2 CameraZoomMax = new Vector2(-5, -1);
    public float zoomInput;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        //offset = transform.position - playerTransform.position;
    }

    void FixedUpdate()
    {
        smoothing = ((PlayerPrefs.GetFloat("camsensitivity", 1f) / PlayerPrefs.GetFloat("camsensitivitymax", 1f)) * 2.5f) + 6f;
        rotation.y = Mathf.Clamp(rotation.y, CameraRotMax.x, CameraRotMax.y);
        Vector3 desiredPosition = playerTransform.position + Quaternion.Euler(rotation.y * .8f, rotation.x, 0) * offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothing * Time.deltaTime);
        transform.position = smoothedPosition;

        transform.LookAt(playerTransform);
        OnCameraZoom();
        OnCameraRotate();
    }
    private void Update()
    {

        // lerpLookAt = new Vector3(Mathf.Lerp(lerpLookAt.x, currentLookAt.x, lookAtSpeed * Time.deltaTime), Mathf.Lerp(lerpLookAt.y, currentLookAt.y, lookAtSpeed * Time.deltaTime), Mathf.Lerp(lerpLookAt.z, currentLookAt.z, lookAtSpeed * Time.deltaTime));
        camRotateSensitivity = PlayerPrefs.GetFloat("camsensitivity");


    }
    public void OnCameraZoom()
    {
        zoomInput = Input.GetAxis("Zoom");
        // Zoom in or out based on the input value and zoom speed
        offset = offset * (1f - zoomInput * Time.deltaTime * zoomSpeed);
        ClampOffset();

    }
    public void ClampOffset()
    {
        offset.x = Mathf.Clamp(offset.x, CameraZoomMax.x, CameraZoomMax.y);
        offset.y = offset.x;
        offset.z = offset.x * zoomDistanceScale;
    }

    public void OnCameraRotate()
    {
        Vector2 rotateInput = new Vector2(Input.GetAxis("CameraHorizontal"), Input.GetAxis("CameraVertical"));
        // Rotate around the player based on the input value and rotation speed
        rotation += rotateInput * Time.deltaTime * rotationSpeed * PlayerPrefs.GetFloat("camsensitivity");
    }
}