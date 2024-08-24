using UnityEngine;

[ExecuteInEditMode]
public class TransformExtension : MonoBehaviour
{
    public Vector3 targetLength = Vector3.one;  // Target length in world units
    public float snapValue = 1.0f;              // Snap value for length adjustments
    public bool snappingEnabled = true;         // Toggle snapping on or off

    private void Update()
    {
        AdjustScale();
    }

    public void AdjustScale()
    {
        Vector3 currentSize = GetCurrentSize();
        Vector3 scaleAdjustment = new Vector3(
            targetLength.x / currentSize.x,
            targetLength.y / currentSize.y,
            targetLength.z / currentSize.z
        );

        Vector3 newScale = new Vector3(
            scaleAdjustment.x * transform.localScale.x,
            scaleAdjustment.y * transform.localScale.y,
            scaleAdjustment.z * transform.localScale.z
        );

        if (snappingEnabled)
        {
            newScale = SnapToMultiple(newScale, snapValue);
        }

        transform.localScale = newScale;
    }

    public Vector3 GetCurrentSize()
    {
        if (TryGetComponent(out Renderer renderer))
        {
            return renderer.bounds.size;
        }

        Debug.LogWarning("Renderer not found! Defaulting size to Vector3.one");
        return Vector3.one;
    }

    private Vector3 SnapToMultiple(Vector3 value, float snap)
    {
        return new Vector3(
            Mathf.Round(value.x / snap) * snap,
            Mathf.Round(value.y / snap) * snap,
            Mathf.Round(value.z / snap) * snap
        );
    }
}
