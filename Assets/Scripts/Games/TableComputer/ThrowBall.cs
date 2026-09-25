using System;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ThrowBall : MonoBehaviour
{
    [SerializeField, MinMaxSlider(0f, 1f)] private Vector2 perfectZone = new Vector2(0.4f, 0.5f);
    [SerializeField, Range(1f, 89f)] private float launchAngle = 60f;
    [SerializeField, Tag] private string obstacleTag = "Obstacle";

    private Rigidbody2D _rb;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private float _initialGravityScale;

    public event Action<ThrowBall> HitObstacle;

    public float Gravity => Physics2D.gravity.y * _initialGravityScale;
    public Vector2 PerfectZone => perfectZone;
    public float LaunchAngle => launchAngle;

    public bool IsTouching(Collider2D surface) => _rb.IsTouching(surface);

    public bool IsSettled(float linearThreshold, float angularThreshold)
    {
        return _rb.linearVelocity.magnitude <= linearThreshold && Mathf.Abs(_rb.angularVelocity) <= angularThreshold;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
        _initialGravityScale = _rb.gravityScale;
        _rb.gravityScale = 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryReportObstacle(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryReportObstacle(other);
    }

    public void PrepareAndActivate()
    {
        gameObject.SetActive(true);

        _rb.gravityScale = 0f;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
    }

    public void Launch(Vector2 velocity)
    {
        _rb.gravityScale = _initialGravityScale;
        _rb.linearVelocity = velocity;
    }

    public void Hide()
    {
        _rb.gravityScale = 0f;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        gameObject.SetActive(false);
    }

    private void TryReportObstacle(Collider2D other)
    {
        if (other.CompareTag(obstacleTag))
        {
            HitObstacle?.Invoke(this);
        }
    }
}
