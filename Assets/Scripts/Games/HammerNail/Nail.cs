using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Nail : MonoBehaviour
{
    [SerializeField] private float launchSpeed = 10f;

    [SerializeField, Required] private HammerNailGameManager gameManager;

    public bool IsFlying { get; private set; }

    private Rigidbody2D _rb;
    private Transform _initialParent;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _initialParent = transform.parent;
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    public void Launch()
    {
        IsFlying = true;
        _rb.linearVelocity = Vector2.up * launchSpeed;
    }

    public void Hide()
    {
        IsFlying = false;
        transform.SetParent(_initialParent);
        _rb.bodyType = RigidbodyType2D.Dynamic;
        gameObject.SetActive(false);
    }

    public void PrepareAndActivate()
    {
        gameObject.SetActive(true);

        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsFlying)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Log"))
        {
            EmbedInLog(collision.transform);
        }
        else if (collision.gameObject.CompareTag("Nail"))
        {
            FreezeInPlace();
            gameManager.OnNailHitNail();
        }
    }

    private void EmbedInLog(Transform log)
    {
        FreezeInPlace();
        transform.SetParent(log);
        gameManager.OnNailHitLog();
    }

    private void FreezeInPlace()
    {
        IsFlying = false;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.bodyType = RigidbodyType2D.Kinematic;
    }
}
