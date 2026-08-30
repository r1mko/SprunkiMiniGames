using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFrameAdapter : MonoBehaviour
{
    [SerializeField, Required] private Transform frame; // authored at local size 1x1 (-0.5..0.5), world scale = world size
    [SerializeField] private float baseFieldOfView = 60f;

    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Start()
    {
        FitFrame();
    }

    [Button("Fit To Frame")]
    private void FitFrame()
    {
        if (_camera == null)
        {
            _camera = GetComponent<Camera>();
        }

        float halfWidth = frame.lossyScale.x * 0.5f;
        float halfHeight = frame.lossyScale.y * 0.5f;
        float distance = Mathf.Abs(frame.position.z - _camera.transform.position.z);

        float requiredHalfHeight = Mathf.Max(halfHeight, halfWidth / _camera.aspect);
        float requiredFov = 2f * Mathf.Atan(requiredHalfHeight / distance) * Mathf.Rad2Deg;

        _camera.fieldOfView = Mathf.Max(requiredFov, baseFieldOfView);
    }
}
