using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShootBalloon : MonoBehaviour
{
    [SerializeField, Required] private GameObject poppedBalloon;

    private Vector3 _poppedInitialLocalPosition;
    private Quaternion _poppedInitialLocalRotation;
    private bool _isInitialized;

    public bool IsPopped { get; private set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPopped || !other.TryGetComponent<ShootNeedle>(out _))
        {
            return;
        }

        Pop();
    }

    public void ResetBalloon()
    {
        EnsureInitialized();
        IsPopped = false;

        poppedBalloon.transform.localPosition = _poppedInitialLocalPosition;
        poppedBalloon.transform.localRotation = _poppedInitialLocalRotation;
        poppedBalloon.SetActive(false);
        gameObject.SetActive(true);
    }

    private void EnsureInitialized()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        _poppedInitialLocalPosition = poppedBalloon.transform.localPosition;
        _poppedInitialLocalRotation = poppedBalloon.transform.localRotation;
    }

    private void Pop()
    {
        IsPopped = true;
        gameObject.SetActive(false);
        poppedBalloon.SetActive(true);
    }
}
