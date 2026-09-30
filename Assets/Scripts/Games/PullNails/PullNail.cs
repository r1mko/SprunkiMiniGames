using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PullNail : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private float _initialGravityScale;
    private bool _isInitialized;

    public bool IsPulled { get; private set; }

    public void Grab()
    {
        EnsureInitialized();
        IsPulled = true;
    }

    public void MoveTo(Vector3 position)
    {
        transform.position = position;
    }

    public void Drop()
    {
        _rb.gravityScale = _initialGravityScale;
    }

    public void ResetNail()
    {
        EnsureInitialized();
        IsPulled = false;

        _rb.gravityScale = 0f;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
    }

    private void EnsureInitialized()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        _rb = GetComponent<Rigidbody2D>();
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
        _initialGravityScale = _rb.gravityScale;
    }
}
